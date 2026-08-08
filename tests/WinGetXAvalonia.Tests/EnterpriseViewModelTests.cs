// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using Avalonia.Platform.Storage;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Backup;
using WinGetXAvalonia.Services.Core;
using WinGetXAvalonia.Services.Dialogs;
using WinGetXAvalonia.Services.Enterprise;
using WinGetXAvalonia.Services.History;
using WinGetXAvalonia.Services.Interfaces;
using WinGetXAvalonia.Services.Notifications;
using WinGetXAvalonia.ViewModels;

namespace WinGetXAvalonia.Tests;

public class EnterpriseViewModelTests
{
    private sealed class FakeFilePickerService : IFilePickerService
    {
        public Task<IStorageFile?> PickOpenJsonAsync(string title, CancellationToken cancellationToken = default)
            => Task.FromResult<IStorageFile?>(null);

        public Task<IStorageFile?> PickSaveJsonAsync(string title, string suggestedFileName, CancellationToken cancellationToken = default)
            => Task.FromResult<IStorageFile?>(null);

        public Task<IStorageFolder?> PickFolderAsync(string title, CancellationToken cancellationToken = default)
            => Task.FromResult<IStorageFolder?>(null);
    }

    private sealed class FakeDialogService : IDialogService
    {
        public bool ConfirmationResult { get; set; } = true;

        public Task<bool> ConfirmAsync(string title, string message, string? confirmText = null, string? cancelText = null)
            => Task.FromResult(ConfirmationResult);

        public Task ProgressAsync(string title, Func<IProgress<double>, Task> operation)
            => Task.CompletedTask;

        public Task ShowErrorAsync(string title, string message)
            => Task.CompletedTask;
    }

    private sealed class FakeNotificationService : INotificationService
    {
        public List<(string Title, string Message)> Successes { get; } = new();

        public List<(string Title, string Message)> Errors { get; } = new();

        public List<(string Title, string Message)> Infos { get; } = new();

        public void ShowSuccess(string title, string message) => Successes.Add((title, message));

        public void ShowError(string title, string message) => Errors.Add((title, message));

        public void ShowWarning(string title, string message) { }

        public void ShowInfo(string title, string message) => Infos.Add((title, message));
    }

    private sealed class FakeHistoryService : IHistoryService
    {
        public List<HistoryEntry> Entries { get; } = new();

        public Task AddAsync(HistoryEntry entry)
        {
            Entries.Add(entry);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<HistoryEntry>> GetAllAsync()
            => Task.FromResult<IReadOnlyList<HistoryEntry>>(Entries.ToList());

        public Task ClearAsync()
        {
            Entries.Clear();
            return Task.CompletedTask;
        }
    }

    private static (EnterpriseViewModel Vm, EnterpriseService Service, FakeNotificationService Notifications, FakeHistoryService History, FakeDialogService Dialogs)
        Create()
    {
        var service = new EnterpriseService();
        var notifications = new FakeNotificationService();
        var history = new FakeHistoryService();
        var dialogs = new FakeDialogService();

        var vm = new EnterpriseViewModel(
            service,
            new FleetReportWriter(),
            new FakeFilePickerService(),
            dialogs,
            notifications,
            history,
            new LoggerService());

        return (vm, service, notifications, history, dialogs);
    }

    [Fact]
    public async Task Load_PopulatesFleetAndStats()
    {
        var (vm, service, _, _, _) = Create();

        await vm.LoadAsync();

        var computers = await service.GetComputersAsync();

        Assert.Equal(computers.Count, vm.Total);
        Assert.Equal(computers.Count(c => c.Online), vm.Online);
        Assert.True(vm.IsLoaded);
        Assert.NotNull(vm.SelectedComputer);
    }

    [Fact]
    public async Task Load_Error_ShowsErrorState()
    {
        var vm = new EnterpriseViewModel(
            new ThrowingService(),
            new FleetReportWriter(),
            new FakeFilePickerService(),
            new FakeDialogService(),
            new FakeNotificationService(),
            new FakeHistoryService(),
            new LoggerService());

        await vm.LoadAsync();

        Assert.True(vm.HasError);
        Assert.NotNull(vm.ErrorMessage);
    }

    [Fact]
    public async Task InstallPackage_OnOnlineComputer_Succeeds()
    {
        var (vm, _, notifications, _, _) = Create();
        await vm.LoadAsync();

        var target = vm.FilteredComputers.First(c => c.Online);
        vm.SelectedComputer = target;

        await vm.InstallPackageCommand.ExecuteAsync("Git");

        Assert.Single(notifications.Successes);
        Assert.Contains(vm.Commands, c => c.Payload == "Git");
        Assert.Contains("Git", target.InstalledApps);
    }

    [Fact]
    public async Task InstallPackage_WithNoSelection_DoesNothing()
    {
        var (vm, _, notifications, _, _) = Create();
        await vm.LoadAsync();

        vm.SelectedComputer = null;

        await vm.InstallPackageCommand.ExecuteAsync("Git");

        Assert.Empty(notifications.Errors);
        Assert.Empty(notifications.Successes);
    }

    [Fact]
    public async Task InstallPackage_WithoutSelection_DoesNothing()
    {
        var (vm, _, notifications, _, _) = Create();
        await vm.LoadAsync();
        vm.SelectedComputer = null;

        await vm.InstallPackageCommand.ExecuteAsync("Git");

        Assert.Empty(notifications.Successes);
        Assert.Empty(vm.Commands);
    }

    [Fact]
    public async Task Restart_CancelledByUser_DoesNothing()
    {
        var (vm, _, _, _, dialogs) = Create();
        dialogs.ConfirmationResult = false;
        await vm.LoadAsync();

        vm.SelectedComputer = vm.FilteredComputers.First(c => c.Online);

        await vm.RestartCommand.ExecuteAsync(null);

        Assert.Empty(vm.Commands);
    }

    [Fact]
    public async Task RemoteCommand_IsRecordedInHistory()
    {
        var (vm, _, _, history, _) = Create();
        await vm.LoadAsync();

        vm.SelectedComputer = vm.FilteredComputers.First(c => c.Online);

        await vm.InstallPackageCommand.ExecuteAsync("Google Chrome");

        var entry = Assert.Single(history.Entries);
        Assert.True(entry.Success);
        Assert.Equal("Remote InstallPackage", entry.Operation);
    }

    [Fact]
    public async Task SavePolicy_AddsToCollection()
    {
        var (vm, service, _, _, _) = Create();
        await vm.LoadAsync();

        vm.PolicyName = "No Tor Browser";
        vm.PolicyPackage = "Tor Browser";
        vm.PolicyDescription = "Tor is not allowed.";
        vm.PolicyEnabled = true;
        vm.PolicyActionIndex = 1;

        await vm.SavePolicyCommand.ExecuteAsync(null);

        Assert.Contains(vm.Policies, p => p.Name == "No Tor Browser");
        var policies = await service.GetPoliciesAsync();
        Assert.Contains(policies, p => p.Name == "No Tor Browser");
    }

    [Fact]
    public async Task SavePolicy_EmptyName_IsIgnored()
    {
        var (vm, _, _, _, _) = Create();
        await vm.LoadAsync();

        vm.PolicyName = "";

        await vm.SavePolicyCommand.ExecuteAsync(null);

        Assert.DoesNotContain(vm.Policies, p => p.Name == "");
    }

    [Fact]
    public async Task DeletePolicy_RemovesFromCollection()
    {
        var (vm, _, _, _, dialogs) = Create();
        await vm.LoadAsync();

        var rule = vm.Policies[0];
        vm.SelectedPolicy = rule;

        await vm.DeletePolicyCommand.ExecuteAsync(null);

        Assert.DoesNotContain(vm.Policies, p => p.Id == rule.Id);
        Assert.True(dialogs.ConfirmationResult);
    }

    [Fact]
    public async Task EvaluatePolicies_NoFakeFleetReportsNoViolations()
    {
        var (vm, _, _, _, _) = Create();
        await vm.LoadAsync();

        await vm.EvaluatePoliciesCommand.ExecuteAsync(null);

        Assert.Empty(vm.Violations);
        Assert.False(vm.HasViolations);
    }

    private sealed class ThrowingService : IEnterpriseService
    {
        public Task<IReadOnlyList<CompanyComputer>> GetComputersAsync(CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("boom");

        public Task<FleetStats> GetStatsAsync(CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("boom");

        public Task<RemoteCommand> SendCommandAsync(Guid computerId, RemoteCommandKind kind, string payload, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("boom");

        public Task<IReadOnlyList<RemoteCommand>> GetCommandsAsync(CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("boom");

        public Task<IReadOnlyList<PolicyRule>> GetPoliciesAsync(CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("boom");

        public Task SavePolicyAsync(PolicyRule rule, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task DeletePolicyAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<IReadOnlyList<PolicyViolation>> EvaluatePoliciesAsync(CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("boom");
    }
}
