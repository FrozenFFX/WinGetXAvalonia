// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using WinGetXAvalonia.Core.Operations;
using WinGetXAvalonia.Core.Providers;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Core;
using WinGetXAvalonia.Services.History;
using WinGetXAvalonia.Services.Notifications;
using WinGetXAvalonia.Services.Operations;
using WinGetXAvalonia.ViewModels;

namespace WinGetXAvalonia.Tests;

public class CommandPaletteViewModelTests
{
    private sealed class FakePackageProvider : IPackageProvider
    {
        public IReadOnlyList<PackageModel> SearchResults { get; set; } = new List<PackageModel>();

        public List<string> Searched { get; } = new();

        public Task<IReadOnlyList<PackageModel>> SearchAsync(string query)
        {
            Searched.Add(query);
            return Task.FromResult(SearchResults);
        }

        public Task InstallAsync(string id, IOperationProgress? progress = null)
            => Task.CompletedTask;

        public Task UpgradeAsync(string id, IOperationProgress? progress = null)
            => Task.CompletedTask;

        public Task UninstallAsync(string id, IOperationProgress? progress = null)
            => Task.CompletedTask;

        public Task<IReadOnlyList<InstalledPackage>> GetInstalledAsync()
            => Task.FromResult<IReadOnlyList<InstalledPackage>>(Array.Empty<InstalledPackage>());

        public Task<IReadOnlyList<UpdatePackage>> GetUpdatesAsync()
            => Task.FromResult<IReadOnlyList<UpdatePackage>>(Array.Empty<UpdatePackage>());

        public Task<string?> GetHomepageAsync(string id)
            => Task.FromResult<string?>(null);

        public Task<PackageDetails> GetDetailsAsync(string id)
            => Task.FromResult(new PackageDetails());
    }

    private sealed class FakeOperationService : IOperationService
    {
        public List<string> Installed { get; } = new();

        public List<string> Updated { get; } = new();

        public List<string> Removed { get; } = new();

        public int UpgradeAllCalls { get; private set; }

        public Task InstallAsync(string packageId)
        {
            Installed.Add(packageId);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(string packageId)
        {
            Updated.Add(packageId);
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string packageId)
        {
            Removed.Add(packageId);
            return Task.CompletedTask;
        }

        public Task UpgradeAllAsync()
        {
            UpgradeAllCalls++;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeHistoryService : IHistoryService
    {
        public List<HistoryEntry> Entries { get; } = new();

        public int ClearCalls { get; private set; }

        public Task AddAsync(HistoryEntry entry)
        {
            Entries.Add(entry);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<HistoryEntry>> GetAllAsync()
            => Task.FromResult<IReadOnlyList<HistoryEntry>>(Entries.ToList());

        public Task ClearAsync()
        {
            ClearCalls++;
            Entries.Clear();
            return Task.CompletedTask;
        }
    }

    private static (CommandPaletteViewModel vm, FakePackageProvider provider, FakeOperationService operations, FakeHistoryService history) Create()
    {
        var provider = new FakePackageProvider();
        var operations = new FakeOperationService();
        var history = new FakeHistoryService();

        var vm = new CommandPaletteViewModel(
            new NavigationService(),
            provider,
            new PackageSearchCache(TimeSpan.Zero),
            operations,
            history,
            new LoggerService(),
            new NotificationService())
        {
            DebounceDelay = TimeSpan.Zero
        };

        return (vm, provider, operations, history);
    }

    private static PackageModel Package(string name, string id)
        => new() { Name = name, Id = id };

    private static async Task WaitForAsync(Func<bool> condition, int timeoutMs = 3000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);

        while (!condition())
        {
            if (DateTime.UtcNow >= deadline)
                throw new TimeoutException("Timed out waiting for palette results.");

            await Task.Delay(10);
        }
    }

    private static void Select(CommandPaletteViewModel vm, string title, CommandPaletteAction? action = null)
    {
        var item = vm.Results.FirstOrDefault(i => i.Title == title
            && (!action.HasValue || i.Action == action.Value));

        Assert.NotNull(item);
        vm.SelectedIndex = vm.Results.IndexOf(item!);
    }

    [Fact]
    public async Task Open_BuildsActionsAndRecentPackages()
    {
        var (vm, _, _, history) = Create();

        history.Entries.Add(new HistoryEntry { PackageId = "Git.Git", PackageName = "Git" });
        history.Entries.Add(new HistoryEntry { PackageId = "Google.Chrome", PackageName = "Google Chrome" });
        history.Entries.Add(new HistoryEntry { PackageId = "Git.Git", PackageName = "Git" });

        vm.Open();

        await WaitForAsync(() => vm.Results.Any(i =>
            i.Action == CommandPaletteAction.InstallPackage
            && i.Parameter == "Git.Git"));

        Assert.True(vm.IsOpen);
        Assert.Contains(vm.Results, i => i.Title == "Install" && i.Action == CommandPaletteAction.StartInstall);
        Assert.Contains(vm.Results, i => i.Title == "Open History" && i.Action == CommandPaletteAction.OpenPage);

        var recents = vm.Results
            .Where(i => i.Action == CommandPaletteAction.InstallPackage && i.Parameter is not null)
            .Select(i => i.Parameter)
            .ToList();

        Assert.Equal(2, recents.Distinct().Count());
    }

    [Fact]
    public async Task CommandMode_ShowsInstallCommandWithArgument()
    {
        var (vm, provider, _, _) = Create();
        provider.SearchResults = new List<PackageModel> { Package("Visual Studio Code", "Microsoft.VisualStudioCode") };

        vm.Open();
        vm.SearchText = ">install vscode";

        await WaitForAsync(() => vm.Results.Any(i => i.Title == "Install vscode"));

        var item = vm.Results.First(i => i.Title == "Install vscode");

        Assert.Equal(CommandPaletteAction.InstallPackage, item.Action);
        Assert.Equal("vscode", item.Parameter);
    }

    [Fact]
    public async Task InstallCommand_ResolvesIdAndInstalls()
    {
        var (vm, provider, operations, _) = Create();
        provider.SearchResults = new List<PackageModel> { Package("Visual Studio Code", "Microsoft.VisualStudioCode") };

        vm.NavigateToPage = _ => true;
        vm.Open();
        vm.SearchText = ">install vscode";

        await WaitForAsync(() => vm.Results.Any(i => i.Title == "Install vscode"));

        Select(vm, "Install vscode");
        await vm.ExecuteCommand.ExecuteAsync(null);

        Assert.Contains("Microsoft.VisualStudioCode", operations.Installed);
        Assert.False(vm.IsOpen);
    }

    [Fact]
    public async Task PackageQuery_EnterOnResult_Installs()
    {
        var (vm, provider, operations, _) = Create();
        provider.SearchResults = new List<PackageModel> { Package("Git", "Git.Git") };

        vm.NavigateToPage = _ => true;
        vm.Open();
        vm.SearchText = "git";

        await WaitForAsync(() => vm.Results.Any(i => !i.IsHeader && i.Title == "Git"));

        Select(vm, "Git");
        await vm.ExecuteCommand.ExecuteAsync(null);

        Assert.Contains("Git.Git", operations.Installed);
        Assert.False(vm.IsOpen);
    }

    [Fact]
    public async Task OpenSettings_Command_NavigatesAndCloses()
    {
        var (vm, _, _, _) = Create();
        var navigated = new List<Type>();
        vm.NavigateToPage = type =>
        {
            navigated.Add(type);
            return true;
        };

        vm.Open();
        vm.SearchText = ">settings";

        await WaitForAsync(() => vm.Results.Any(i => i.Title == "Open Settings"));

        Select(vm, "Open Settings");
        await vm.ExecuteCommand.ExecuteAsync(null);

        Assert.Contains(typeof(SettingsViewModel), navigated);
        Assert.False(vm.IsOpen);
    }

    [Fact]
    public async Task ClearHistory_Command_ClearsHistory()
    {
        var (vm, _, _, history) = Create();
        history.Entries.Add(new HistoryEntry { PackageId = "Git.Git" });

        vm.Open();
        vm.SearchText = ">clear history";

        await WaitForAsync(() => vm.Results.Any(i => i.Title == "Clear history"));

        Select(vm, "Clear history");
        await vm.ExecuteCommand.ExecuteAsync(null);

        Assert.Equal(1, history.ClearCalls);
        Assert.False(vm.IsOpen);
    }

    [Fact]
    public async Task SearchCommand_InvokesSearchDelegate()
    {
        var (vm, _, _, _) = Create();
        string? searchQuery = null;
        vm.SearchPackages = query => searchQuery = query;

        vm.Open();
        vm.SearchText = ">search docker";

        await WaitForAsync(() => vm.Results.Any(i => i.Title == "Search for docker"));

        Select(vm, "Search for docker");
        await vm.ExecuteCommand.ExecuteAsync(null);

        Assert.Equal("docker", searchQuery);
        Assert.False(vm.IsOpen);
    }

    [Fact]
    public async Task StartInstall_Action_ChainsToCommandText()
    {
        var (vm, _, _, _) = Create();

        vm.Open();

        await WaitForAsync(() => vm.Results.Any(i =>
            i.Title == "Install" && i.Action == CommandPaletteAction.StartInstall));

        Select(vm, "Install", CommandPaletteAction.StartInstall);
        await vm.ExecuteCommand.ExecuteAsync(null);

        Assert.Equal("install ", vm.SearchText);
        Assert.True(vm.IsOpen);
    }

    [Fact]
    public async Task MoveDown_MoveUp_SkipHeaders()
    {
        var (vm, _, _, history) = Create();
        history.Entries.Add(new HistoryEntry { PackageId = "Git.Git", PackageName = "Git" });

        vm.Open();

        await WaitForAsync(() => vm.Results.Any(i => !i.IsHeader));

        vm.SelectedIndex = vm.Results.IndexOf(vm.Results.First(i => !i.IsHeader));

        int previous = vm.SelectedIndex;

        vm.MoveDownCommand.Execute(null);

        Assert.NotEqual(previous, vm.SelectedIndex);
        Assert.False(vm.Results[vm.SelectedIndex].IsHeader);

        vm.SelectedIndex = vm.Results.Count - 1;

        vm.MoveUpCommand.Execute(null);

        Assert.False(vm.Results[vm.SelectedIndex].IsHeader);
    }

    [Fact]
    public async Task Toggle_OpenThenClose()
    {
        var (vm, _, _, _) = Create();

        vm.ToggleCommand.Execute(null);
        Assert.True(vm.IsOpen);

        vm.ToggleCommand.Execute(null);
        Assert.False(vm.IsOpen);
        Assert.Empty(vm.Results);
    }
}
