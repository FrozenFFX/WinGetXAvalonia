// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Blueprints;
using WinGetXAvalonia.Services.Core;
using WinGetXAvalonia.Services.Dialogs;
using WinGetXAvalonia.Services.History;
using WinGetXAvalonia.Services.Marketplace;
using WinGetXAvalonia.Services.Notifications;
using WinGetXAvalonia.Services.Theme;
using WinGetXAvalonia.Services.Workspaces;
using WinGetXAvalonia.ViewModels;

namespace WinGetXAvalonia.Tests;

public class MarketplaceViewModelTests
{
    private static MarketplaceCatalog CreateCatalog()
    {
        return new MarketplaceCatalog
        {
            Version = 1,
            Items =
            [
                new MarketplaceItem
                {
                    Id = "bp-dev",
                    Kind = MarketplaceKind.Blueprint,
                    Name = "Dev Setup",
                    Description = "Developer toolchain",
                    Author = "FrozenFFX",
                    Tags = new List<string> { "developer" },
                    Rating = 4.9,
                    RatingCount = 50,
                    Payload = "{\"Name\":\"Dev Setup\",\"Description\":\"d\",\"Steps\":\"1\"}"
                },
                new MarketplaceItem
                {
                    Id = "bp-game",
                    Kind = MarketplaceKind.Blueprint,
                    Name = "Gaming",
                    Description = "Gaming tools",
                    Author = "Community",
                    Tags = new List<string> { "gaming" },
                    Payload = "{\"Name\":\"Gaming\",\"Description\":\"d\",\"Steps\":\"1\"}"
                },
                new MarketplaceItem
                {
                    Id = "ws-designer",
                    Kind = MarketplaceKind.Workspace,
                    Name = "Designer Pack",
                    Description = "Design tools",
                    Author = "Community",
                    Payload = "{\"Name\":\"Designer Pack\",\"Description\":\"d\",\"Packages\":[{\"Id\":\"Figma.Figma\",\"Name\":\"Figma\"}]}"
                },
                new MarketplaceItem
                {
                    Id = "th-solar",
                    Kind = MarketplaceKind.Theme,
                    Name = "Solarized",
                    Description = "Warm accent",
                    Author = "Community",
                    Payload = "{\"Name\":\"Solarized\",\"Accent\":\"#B58900\",\"Description\":\"warm\",\"Dark\":true}"
                }
            ]
        };
    }

    private sealed class FakeBlueprintStore : IBlueprintStore
    {
        public List<BlueprintModel> Items { get; } = new();

        public List<string> ImportedPayloads { get; } = new();

        public bool ThrowOnImport { get; set; }

        public bool ThrowOnGetAll { get; set; }

        public Task<IReadOnlyList<BlueprintModel>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            if (ThrowOnGetAll)
                throw new InvalidOperationException("boom");

            return Task.FromResult<IReadOnlyList<BlueprintModel>>(Items.ToList());
        }

        public Task SaveAsync(BlueprintModel blueprint, CancellationToken cancellationToken = default)
        {
            Items.RemoveAll(i => i.Id == blueprint.Id);
            Items.Add(blueprint);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            Items.RemoveAll(i => i.Id == id);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<BlueprintModel>> ImportJsonAsync(string json, CancellationToken cancellationToken = default)
        {
            if (ThrowOnImport)
                throw new InvalidOperationException("boom");

            ImportedPayloads.Add(json);
            return Task.FromResult<IReadOnlyList<BlueprintModel>>(new List<BlueprintModel>());
        }
    }

    private sealed class FakeWorkspaceService : IWorkspaceService
    {
        public List<WorkspaceModel> Items { get; } = new();

        public List<string> ImportedPayloads { get; } = new();

        public Task<IReadOnlyList<WorkspaceModel>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<WorkspaceModel>>(Items.ToList());

        public Task<WorkspaceModel?> GetAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult<WorkspaceModel?>(null);

        public Task SaveAsync(WorkspaceModel workspace, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task ImportAsync(string file, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task ImportJsonAsync(string json, CancellationToken cancellationToken = default)
        {
            ImportedPayloads.Add(json);
            return Task.CompletedTask;
        }

        public Task ExportAsync(Guid id, string file, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task EnsureExampleWorkspacesAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeNotificationService : INotificationService
    {
        public List<(string Title, string Message)> Successes { get; } = new();

        public List<(string Title, string Message)> Errors { get; } = new();

        public void ShowSuccess(string title, string message) => Successes.Add((title, message));

        public void ShowError(string title, string message) => Errors.Add((title, message));

        public void ShowWarning(string title, string message) { }

        public void ShowInfo(string title, string message) { }
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

    private sealed class FakeDialogService : IDialogService
    {
        public Task<bool> ConfirmAsync(string title, string message, string? confirmText = null, string? cancelText = null)
            => Task.FromResult(true);

        public Task ProgressAsync(string title, Func<IProgress<double>, Task> operation)
            => Task.CompletedTask;

        public Task ShowErrorAsync(string title, string message)
            => Task.CompletedTask;
    }

    private static (MarketplaceViewModel Vm, FakeBlueprintStore Blueprints, FakeWorkspaceService Workspaces, FakeNotificationService Notifications, FakeHistoryService History, ThemeManager Theme, string SettingsFile)
        Create()
    {
        var dir = Path.Combine(
            Path.GetTempPath(),
            "WinGetXAvalonia.Tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);

        var file = Path.Combine(dir, "settings.json");
        var settings = new SettingsService(new SettingsStore(file));
        var theme = new ThemeService(settings);
        var themeManager = new ThemeManager(settings, theme);

        var blueprints = new FakeBlueprintStore();
        var workspaces = new FakeWorkspaceService();
        var notifications = new FakeNotificationService();
        var history = new FakeHistoryService();

        var vm = new MarketplaceViewModel(
            new MarketplaceService(CreateCatalog()),
            blueprints,
            workspaces,
            themeManager,
            new FakeDialogService(),
            notifications,
            history,
            new LoggerService());

        return (vm, blueprints, workspaces, notifications, history, themeManager, file);
    }

    [Fact]
    public async Task Load_ShowsBlueprintsByDefault()
    {
        var (vm, _, _, _, _, _, _) = Create();

        await vm.LoadAsync();

        Assert.Equal(MarketplaceKind.Blueprint, vm.SelectedTab!.Kind);
        Assert.Equal(2, vm.Items.Count);
        Assert.True(vm.IsLoaded);
    }

    [Fact]
    public async Task SwitchingTab_FiltersByKind()
    {
        var (vm, _, _, _, _, _, _) = Create();
        await vm.LoadAsync();

        vm.SelectedTab = vm.Tabs.First(t => t.Kind == MarketplaceKind.Workspace);

        Assert.Single(vm.Items);
        Assert.Equal(MarketplaceKind.Workspace, vm.Items[0].Kind);
    }

    [Fact]
    public async Task Search_FiltersItemsByName()
    {
        var (vm, _, _, _, _, _, _) = Create();
        await vm.LoadAsync();

        vm.SearchText = "gaming";

        Assert.Single(vm.Items);
        Assert.Equal("Gaming", vm.Items[0].Name);
    }

    [Fact]
    public async Task Search_NoMatch_ShowsEmpty()
    {
        var (vm, _, _, _, _, _, _) = Create();
        await vm.LoadAsync();

        vm.SearchText = "zzz-unknown";

        Assert.True(vm.IsEmpty);
        Assert.True(vm.HasNoItems);
    }

    [Fact]
    public async Task InstallBlueprint_ImportsPayloadToStore()
    {
        var (vm, blueprints, _, _, _, _, _) = Create();
        await vm.LoadAsync();

        var item = vm.Items.First(i => i.Id == "bp-dev");

        await vm.InstallCommand.ExecuteAsync(item);

        Assert.Single(blueprints.ImportedPayloads);
        Assert.True(item.IsInstalled);
        Assert.False(item.IsInstalling);
    }

    [Fact]
    public async Task InstallWorkspace_ImportsPayload()
    {
        var (vm, _, workspaces, _, _, _, _) = Create();
        await vm.LoadAsync();
        vm.SelectedTab = vm.Tabs.First(t => t.Kind == MarketplaceKind.Workspace);

        var item = vm.Items[0];

        await vm.InstallCommand.ExecuteAsync(item);

        Assert.Single(workspaces.ImportedPayloads);
        Assert.True(item.IsInstalled);
    }

    [Fact]
    public async Task InstallWorkspace_PayloadIsValidWorkspaceJson()
    {
        var (vm, _, workspaces, _, _, _, _) = Create();
        await vm.LoadAsync();
        vm.SelectedTab = vm.Tabs.First(t => t.Kind == MarketplaceKind.Workspace);

        var item = vm.Items[0];
        var serializer = new WorkspaceSerializer();

        await vm.InstallCommand.ExecuteAsync(item);

        var payload = Assert.Single(workspaces.ImportedPayloads);
        var workspace = serializer.Deserialize(payload);

        Assert.Equal(item.Name, workspace.Name);
        Assert.NotEmpty(workspace.Packages);
    }

    [Fact]
    public async Task InstallTheme_RegistersAccent()
    {
        var (vm, _, _, _, _, themeManager, _) = Create();
        await vm.LoadAsync();
        vm.SelectedTab = vm.Tabs.First(t => t.Kind == MarketplaceKind.Theme);

        var item = vm.Items[0];

        await vm.InstallCommand.ExecuteAsync(item);

        Assert.True(item.IsInstalled);
        Assert.Contains(themeManager.AllAccents, a => a.Name == "Solarized");
    }

    [Fact]
    public async Task InstallTheme_PersistsToSettings()
    {
        var (vm, _, _, _, _, _, settingsFile) = Create();
        await vm.LoadAsync();
        vm.SelectedTab = vm.Tabs.First(t => t.Kind == MarketplaceKind.Theme);

        var item = vm.Items[0];

        await vm.InstallCommand.ExecuteAsync(item);

        var reloaded = new SettingsStore(settingsFile);
        await reloaded.LoadAsync();

        Assert.Contains(
            reloaded.Settings.InstalledAccents,
            a => a.Name == "Solarized" && a.Hex == "#B58900");
    }

    [Fact]
    public async Task InstallAlreadyInstalled_IsSkipped()
    {
        var (vm, blueprints, _, _, _, _, _) = Create();
        await vm.LoadAsync();

        var item = vm.Items.First(i => i.Id == "bp-dev");
        item.IsInstalled = true;

        await vm.InstallCommand.ExecuteAsync(item);

        Assert.Empty(blueprints.ImportedPayloads);
    }

    [Fact]
    public async Task InstallFailure_NotifiesAndRecordsHistory()
    {
        var (vm, blueprints, _, notifications, history, _, _) = Create();
        blueprints.ThrowOnImport = true;
        await vm.LoadAsync();

        var item = vm.Items.First(i => i.Id == "bp-dev");

        await vm.InstallCommand.ExecuteAsync(item);

        Assert.Single(notifications.Errors);
        Assert.Single(history.Entries);
        Assert.False(history.Entries[0].Success);
        Assert.False(item.IsInstalled);
        Assert.False(item.IsInstalling);
    }

    [Fact]
    public async Task LoadError_SetsErrorState()
    {
        var (vm, blueprints, _, _, _, _, _) = Create();
        blueprints.ThrowOnGetAll = true;

        await vm.LoadAsync();

        Assert.True(vm.HasError);
        Assert.NotNull(vm.ErrorMessage);
    }
}
