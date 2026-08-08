// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using Avalonia.Platform.Storage;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Backup;
using WinGetXAvalonia.Services.Core;
using WinGetXAvalonia.Services.Dialogs;
using WinGetXAvalonia.Services.History;
using WinGetXAvalonia.Services.Identity;
using WinGetXAvalonia.Services.Interfaces;
using WinGetXAvalonia.Services.Notifications;
using WinGetXAvalonia.ViewModels;

namespace WinGetXAvalonia.Tests;

public class SoftwareIdentityViewModelTests : IDisposable
{
    private readonly string _folder;

    public SoftwareIdentityViewModelTests()
    {
        _folder = Path.Combine(
            Path.GetTempPath(),
            "WinGetXAvalonia.Tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_folder);
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(_folder, recursive: true);
        }
        catch (IOException)
        {
            // test temizliği ikincil; başarısızsa görmezden gel
        }
    }

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

        public List<(string Title, string Message)> Warnings { get; } = new();

        public void ShowSuccess(string title, string message) => Successes.Add((title, message));

        public void ShowError(string title, string message) => Errors.Add((title, message));

        public void ShowWarning(string title, string message) => Warnings.Add((title, message));

        public void ShowInfo(string title, string message) { }
    }

    private sealed class FakeHistoryService : IHistoryService
    {
        public Task AddAsync(HistoryEntry entry)
            => Task.CompletedTask;

        public Task<IReadOnlyList<HistoryEntry>> GetAllAsync()
            => Task.FromResult<IReadOnlyList<HistoryEntry>>(new List<HistoryEntry>());

        public Task ClearAsync()
            => Task.CompletedTask;
    }

    private sealed class FakeExportService : IExportService
    {
        public string? LastFile { get; private set; }

        public Task ExportJsonAsync<T>(IEnumerable<T> items, string file)
        {
            LastFile = file;
            return Task.CompletedTask;
        }
    }

    private static (SoftwareIdentityViewModel Vm, SoftwareIdentityService Service, FakeNotificationService Notifications, FakeDialogService Dialogs)
        Create(string folder)
    {
        var service = new SoftwareIdentityService(folder);
        var notifications = new FakeNotificationService();
        var dialogs = new FakeDialogService();

        var vm = new SoftwareIdentityViewModel(
            service,
            new FakeFilePickerService(),
            dialogs,
            notifications,
            new FakeExportService(),
            new FakeHistoryService(),
            new LoggerService());

        return (vm, service, notifications, dialogs);
    }

    private static SoftwareIdentity Sample(string name, string version = "1.0")
        => new()
        {
            Name = name,
            Publisher = "Test Publisher",
            Version = version,
            Source = "winget"
        };

    [Fact]
    public async Task Load_PopulatesIdentities()
    {
        var (vm, service, _, _) = Create(_folder);
        await service.RecordInstallAsync(Sample("Git"));
        await service.RecordInstallAsync(Sample("Python"));

        await vm.LoadAsync();

        Assert.Equal(2, vm.FilteredIdentities.Count);
        Assert.True(vm.IsLoaded);
        Assert.NotNull(vm.SelectedIdentity);
    }

    [Fact]
    public async Task Load_Empty_ShowsEmptyState()
    {
        var (vm, _, _, _) = Create(_folder);

        await vm.LoadAsync();

        Assert.True(vm.IsEmpty);
        Assert.Empty(vm.FilteredIdentities);
    }

    [Fact]
    public async Task Search_FiltersIdentities()
    {
        var (vm, service, _, _) = Create(_folder);
        await service.RecordInstallAsync(Sample("Git"));
        await service.RecordInstallAsync(Sample("Python"));

        await vm.LoadAsync();

        vm.SearchText = "git";

        Assert.Single(vm.FilteredIdentities);
        Assert.Equal("Git", vm.FilteredIdentities[0].Name);
    }

    [Fact]
    public async Task RecordUpdate_UpdatesSelectedIdentity()
    {
        var (vm, service, notifications, _) = Create(_folder);
        await service.RecordInstallAsync(Sample("Git"));
        await vm.LoadAsync();

        var target = vm.SelectedIdentity!;

        await vm.RecordUpdateCommand.ExecuteAsync(null);

        Assert.Single(notifications.Successes);
        Assert.Equal(1, vm.SelectedIdentity!.UpdateCount);
        Assert.Equal(target.Id, vm.SelectedIdentity.Id);
    }

    [Fact]
    public async Task RecordUpdate_WithoutSelection_DoesNothing()
    {
        var (vm, _, notifications, _) = Create(_folder);
        await vm.LoadAsync();
        vm.SelectedIdentity = null;

        await vm.RecordUpdateCommand.ExecuteAsync(null);

        Assert.Empty(notifications.Successes);
    }

    [Fact]
    public async Task RecordRollback_CancelledByUser_DoesNothing()
    {
        var (vm, service, notifications, dialogs) = Create(_folder);
        dialogs.ConfirmationResult = false;
        await service.RecordInstallAsync(Sample("Git"));
        await vm.LoadAsync();

        var before = vm.SelectedIdentity!.RollbackCount;

        await vm.RecordRollbackCommand.ExecuteAsync(null);

        Assert.Equal(before, vm.SelectedIdentity!.RollbackCount);
        Assert.Empty(notifications.Warnings);
    }

    [Fact]
    public async Task RecordRollback_Confirmed_DecreasesHealth()
    {
        var (vm, service, notifications, _) = Create(_folder);
        var git = await service.RecordInstallAsync(Sample("Git"));
        await service.RecordUpdateAsync(git.Id);
        await vm.LoadAsync();

        var healthBefore = vm.SelectedIdentity!.HealthScore;

        await vm.RecordRollbackCommand.ExecuteAsync(null);

        Assert.Single(notifications.Warnings);
        Assert.Equal(healthBefore - 10, vm.SelectedIdentity!.HealthScore);
    }

    [Fact]
    public async Task Delete_RemovesIdentity()
    {
        var (vm, service, notifications, _) = Create(_folder);
        await service.RecordInstallAsync(Sample("Git"));
        await vm.LoadAsync();

        var count = vm.FilteredIdentities.Count;

        await vm.DeleteCommand.ExecuteAsync(null);

        Assert.Single(notifications.Successes);
        Assert.Equal(count - 1, vm.FilteredIdentities.Count);
    }

    [Fact]
    public async Task Delete_Cancelled_DoesNothing()
    {
        var (vm, service, notifications, dialogs) = Create(_folder);
        dialogs.ConfirmationResult = false;
        await service.RecordInstallAsync(Sample("Git"));
        await vm.LoadAsync();

        var count = vm.FilteredIdentities.Count;

        await vm.DeleteCommand.ExecuteAsync(null);

        Assert.Equal(count, vm.FilteredIdentities.Count);
        Assert.Empty(notifications.Successes);
    }

    [Fact]
    public async Task ExportJson_WithoutPickedFile_DoesNothing()
    {
        var (vm, _, notifications, _) = Create(_folder);
        await vm.LoadAsync();

        await vm.ExportJsonCommand.ExecuteAsync(null);

        Assert.Empty(notifications.Successes);
    }
}
