// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using WinGetXAvalonia.ViewModels;

namespace WinGetXAvalonia.Tests;

[Collection("UiDispatcher")]
public class SidebarRedesignTests
{
    [Fact]
    public async Task Menu_ContainsExactlyOneSelectedItem_AndHeadersAreNotSelectable()
    {
        using var host = Program.CreateHost();

        var vm = await Dispatcher.UIThread.InvokeAsync(() =>
            host.Services.GetRequiredService<MainViewModel>());

        var headers = vm.Menu.Where(m => m.IsCategoryHeader).ToArray();
        var pages = vm.Menu.Where(m => !m.IsCategoryHeader).ToArray();

        Assert.NotEmpty(headers);
        Assert.NotEmpty(pages);
        Assert.All(headers, h => Assert.True(h.IsCategoryHeader));
        Assert.NotNull(vm.SelectedMenuItem);
        Assert.False(vm.SelectedMenuItem!.IsCategoryHeader);
        Assert.Equal(1, vm.Menu.Count(m => m == vm.SelectedMenuItem));
    }

    [Fact]
    public async Task GameModeAndIdentity_UseDistinctIcons()
    {
        using var host = Program.CreateHost();

        var vm = await Dispatcher.UIThread.InvokeAsync(() =>
            host.Services.GetRequiredService<MainViewModel>());

        var gameMode = vm.Menu.First(m => m.Page == typeof(GameModeViewModel));
        var identity = vm.Menu.First(m => m.Page == typeof(SoftwareIdentityViewModel));
        var automation = vm.Menu.First(m => m.Page == typeof(AutomationViewModel));

        Assert.Equal("game", gameMode.Icon);
        Assert.Equal("identity", identity.Icon);
        Assert.Equal("automation", automation.Icon);

        Assert.All(new[] { gameMode.Icon, identity.Icon, automation.Icon },
            icon => Assert.NotEqual("info", icon));
    }

    [Fact]
    public async Task SelectingPageInsideCollapsedCategory_AutoExpandsParent()
    {
        using var host = Program.CreateHost();

        var vm = await Dispatcher.UIThread.InvokeAsync(() =>
            host.Services.GetRequiredService<MainViewModel>());

        var installed = vm.Menu.First(m => m.Page == typeof(InstalledViewModel));
        var header = installed.ParentCategory;

        Assert.NotNull(header);

        // Yeni varsayılan: bölümler kapalı başlar.
        Assert.False(header.IsExpanded);
        Assert.DoesNotContain(installed, vm.VisibleMenu);

        await Dispatcher.UIThread.InvokeAsync(() => vm.SelectedMenuItem = installed);

        Assert.True(header.IsExpanded);
        Assert.Contains(installed, vm.VisibleMenu);
        Assert.Equal(typeof(InstalledViewModel), vm.Navigation.CurrentPage?.GetType());
    }

    [Fact]
    public async Task Version_MatchesAssemblyInformationalVersion()
    {
        using var host = Program.CreateHost();

        var vm = await Dispatcher.UIThread.InvokeAsync(() =>
            host.Services.GetRequiredService<MainViewModel>());

        Assert.False(string.IsNullOrWhiteSpace(vm.Version));
        Assert.Equal("1.0.0", vm.Version);
    }

    [Fact]
    public async Task EveryMenuItem_HasRecognizedIconKey()
    {
        using var host = Program.CreateHost();

        var vm = await Dispatcher.UIThread.InvokeAsync(() =>
            host.Services.GetRequiredService<MainViewModel>());

        foreach (var item in vm.Menu.Where(m => !m.IsCategoryHeader))
        {
            Assert.False(string.IsNullOrWhiteSpace(item.Icon), item.Title + " has no icon");
            Assert.False(item.Icon.Contains(' '), item.Title + " icon contains whitespace");
        }
    }
}