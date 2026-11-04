// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Backup;
using WinGetXAvalonia.Services.Core;
using WinGetXAvalonia.Services.Studio;
using WinGetXAvalonia.ViewModels;

namespace WinGetXAvalonia.Tests;

public class StudioViewModelTests
{
    private sealed class FakeStudioService : IStudioService
    {
        public List<StudioTool> Items { get; } = new();

        public bool ThrowOnGetAll { get; set; }

        public Task<IReadOnlyList<StudioTool>> GetToolsAsync()
        {
            if (ThrowOnGetAll)
                throw new InvalidOperationException("boom");

            return Task.FromResult<IReadOnlyList<StudioTool>>(Items.ToList());
        }
    }

    private static StudioViewModel Create(FakeStudioService? service = null)
        => new(service ?? new FakeStudioService());

    [Fact]
    public async Task Load_PopulatesTools_SortedByTitle()
    {
        var service = new FakeStudioService();
        service.Items.Add(new StudioTool { Title = "Zed", Id = "z" });
        service.Items.Add(new StudioTool { Title = "Alpha", Id = "a" });

        var vm = Create(service);
        await vm.InitializeAsync();

        Assert.Equal(2, vm.Tools.Count);
        Assert.Equal("Alpha", vm.Tools[0].Title);
        Assert.True(vm.IsLoaded);
        Assert.Equal("2 tools", vm.ToolsSummary);
    }

    [Fact]
    public async Task Load_Empty_SetsEmptyState()
    {
        var vm = Create(new FakeStudioService());
        await vm.InitializeAsync();

        Assert.True(vm.IsEmpty);
        Assert.Equal("No tools", vm.ToolsSummary);
    }

    [Fact]
    public async Task Load_Error_SetsErrorState()
    {
        var service = new FakeStudioService { ThrowOnGetAll = true };
        var vm = Create(service);

        await vm.InitializeAsync();

        Assert.True(vm.HasError);
        Assert.Equal("boom", vm.ErrorMessage);
    }

    [Fact]
    public async Task Search_FiltersTools_ByTitle()
    {
        var service = new FakeStudioService();
        service.Items.Add(new StudioTool { Title = "Diagnostics", Id = "d" });
        service.Items.Add(new StudioTool { Title = "Logs", Id = "l" });
        var vm = Create(service);
        await vm.InitializeAsync();

vm.SearchText = "diag";

        Assert.Single(vm.Tools);
        Assert.Equal("Diagnostics", vm.Tools[0].Title);
        Assert.Equal(1, vm.ToolCount);
        Assert.Equal("1 tool", vm.ToolsSummary);
    }

    [Fact]
    public async Task Search_MatchesDescriptionAndCategory()
    {
        var service = new FakeStudioService();
        service.Items.Add(new StudioTool
        {
            Title = "Queue",
            Id = "q",
            Description = "schedule operations",
            Category = "System"
        });
        service.Items.Add(new StudioTool { Title = "Plugins", Id = "p" });
        var vm = Create(service);
        await vm.InitializeAsync();

        vm.SearchText = "schedule";
        Assert.Single(vm.Tools);
        Assert.Equal("Queue", vm.Tools[0].Title);

        vm.SearchText = "system";
        Assert.Single(vm.Tools);
    }

    [Fact]
    public async Task Search_NoMatch_KeepsLoadedState()
    {
        var service = new FakeStudioService();
        service.Items.Add(new StudioTool { Title = "Diagnostics", Id = "d" });
        var vm = Create(service);
        await vm.InitializeAsync();

vm.SearchText = "zzz";

        Assert.Empty(vm.Tools);
        Assert.True(vm.IsLoaded);
        Assert.True(vm.IsFilteredEmpty);
        Assert.Equal("No tools", vm.ToolsSummary);
    }

    [Fact]
    public async Task Filter_Ready_ShowsOnlyReadyTools()
    {
        var service = new FakeStudioService();
        service.Items.Add(new StudioTool { Title = "A", Id = "a", Status = StudioToolStatus.Ready });
        service.Items.Add(new StudioTool { Title = "B", Id = "b", Status = StudioToolStatus.Experimental });
        service.Items.Add(new StudioTool { Title = "C", Id = "c", Status = StudioToolStatus.ComingSoon });
        var vm = Create(service);
        await vm.InitializeAsync();

        vm.SelectFilterCommand.Execute(vm.Filters[1]);

        Assert.Single(vm.Tools);
        Assert.Equal("A", vm.Tools[0].Title);
    }

    [Fact]
    public async Task Filter_All_ShowsEverything()
    {
        var service = new FakeStudioService();
        service.Items.Add(new StudioTool { Title = "A", Id = "a", Status = StudioToolStatus.Ready });
        service.Items.Add(new StudioTool { Title = "B", Id = "b", Status = StudioToolStatus.Experimental });
        var vm = Create(service);
        await vm.InitializeAsync();

        vm.SelectFilterCommand.Execute(vm.Filters[0]);

        Assert.Equal(2, vm.Tools.Count);
    }

    [Fact]
    public async Task Filter_AndSearch_Combine()
    {
        var service = new FakeStudioService();
        service.Items.Add(new StudioTool { Title = "Diagnostics", Id = "d", Status = StudioToolStatus.Ready });
        service.Items.Add(new StudioTool { Title = "Plugins", Id = "p", Status = StudioToolStatus.ComingSoon });
        var vm = Create(service);
        await vm.InitializeAsync();

        vm.SelectFilterCommand.Execute(vm.Filters[1]);
        vm.SearchText = "plugins";

        Assert.Empty(vm.Tools);
    }

    [Fact]
    public async Task CategoryFilter_CombinesWithStatusAndSearch()
    {
        var service = new FakeStudioService();
        service.Items.Add(new StudioTool
        {
            Title = "Diagnostics",
            Id = "diagnostics",
            Category = "System",
            Status = StudioToolStatus.Ready
        });
        service.Items.Add(new StudioTool
        {
            Title = "Automation",
            Id = "automation",
            Category = "Workflow",
            Status = StudioToolStatus.Ready
        });
        var vm = Create(service);
        await vm.InitializeAsync();

        vm.SelectedCategory = "System";

        Assert.Single(vm.Tools);
        Assert.Equal("Diagnostics", vm.Tools[0].Title);
    }

    [Fact]
    public async Task OpenTool_RaisesRequestOnlyForReadyTools()
    {
        var service = new FakeStudioService();
        service.Items.Add(new StudioTool
        {
            Title = "Diagnostics",
            Id = "diagnostics",
            Status = StudioToolStatus.Ready
        });
        service.Items.Add(new StudioTool
        {
            Title = "Plugins",
            Id = "plugins",
            Status = StudioToolStatus.ComingSoon
        });
        var vm = Create(service);
        await vm.InitializeAsync();
        var requested = new List<string>();
        vm.OpenToolRequested += (_, id) => requested.Add(id);

        vm.OpenToolCommand.Execute(vm.Tools.Single(tool => tool.Tool.Id == "diagnostics"));
        vm.OpenToolCommand.Execute(vm.Tools.Single(tool => tool.Tool.Id == "plugins"));

        Assert.Single(requested);
        Assert.Equal("diagnostics", requested[0]);
    }

    [Fact]
    public async Task FavoriteFilter_PersistsFavoriteToolIds()
    {
        var path = Path.Combine(
            Path.GetTempPath(),
            "WinGetXAvalonia.Tests",
            Guid.NewGuid().ToString("N"),
            "settings.json");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        try
        {
            var settings = new SettingsService(new SettingsStore(path));
            await settings.LoadAsync();
            var service = new FakeStudioService();
            service.Items.Add(new StudioTool { Title = "Diagnostics", Id = "diagnostics" });
            service.Items.Add(new StudioTool { Title = "Logs", Id = "logs" });
            var autoSave = new AutoSaveService(settings, TimeSpan.FromMilliseconds(20));
            var vm = new StudioViewModel(service, settings, autoSave);
            await vm.InitializeAsync();

            vm.ToggleFavoriteCommand.Execute(vm.Tools[0]);
            vm.SelectFilterCommand.Execute(vm.Filters.Single(filter => filter.FavoritesOnly));
            await Task.Delay(100);

            Assert.Single(vm.Tools);
            Assert.True(vm.Tools[0].IsFavorite);
            Assert.Contains("diagnostics", settings.Settings.FavoriteStudioTools);

            var reloaded = new SettingsStore(path);
            await reloaded.LoadAsync();
            Assert.Contains("diagnostics", reloaded.Settings.FavoriteStudioTools);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public async Task SelectTool_SetsSelectedTool()
    {
        var service = new FakeStudioService();
        service.Items.Add(new StudioTool { Title = "Diagnostics", Id = "d" });
        var vm = Create(service);
        await vm.InitializeAsync();

        var target = vm.Tools[0];
        vm.SelectToolCommand.Execute(target);

        Assert.Same(target, vm.SelectedTool);
    }

    [Fact]
    public void Badge_ReflectsStatus()
    {
        var ready = new StudioToolViewModel(new StudioTool { Status = StudioToolStatus.Ready });
        Assert.Equal("Ready", ready.Badge);
        Assert.Equal("success", ready.BadgeStatus);

        var experimental = new StudioToolViewModel(new StudioTool { Status = StudioToolStatus.Experimental });
        Assert.Equal("Experimental", experimental.Badge);
        Assert.Equal("warning", experimental.BadgeStatus);

        var comingSoon = new StudioToolViewModel(new StudioTool { Status = StudioToolStatus.ComingSoon });
        Assert.Equal("Coming Soon", comingSoon.Badge);
        Assert.Equal("info", comingSoon.BadgeStatus);
    }
}
