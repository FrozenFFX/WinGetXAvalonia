// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.ViewModels;

namespace WinGetXAvalonia.Tests;

// Sidebar state bug regresyon testleri:
// Sayfa seçimi bölüm genişletme durumuna dokunamaz. Bölüm açma/kapama
// yalnızca kullanıcı kontrolündedir; gezinme "collapse all" komutu değildir.
[Collection("UiDispatcher")]
public class SidebarStateTests
{
    [Fact]
    public async Task FirstLaunch_ExpandsOnlyActiveSection()
    {
        using var host = Program.CreateHost();

        var vm = await Dispatcher.UIThread.InvokeAsync(() =>
            host.Services.GetRequiredService<MainViewModel>());

        var headers = vm.Menu.Where(m => m.IsCategoryHeader).ToArray();

        Assert.NotEmpty(headers);

        // Varsayılan: yalnızca aktif sayfa (Dashboard) HOME açık; gerisi kapalı.
        var expanded = headers.Where(h => h.IsExpanded).ToArray();

        Assert.Single(expanded);
        Assert.Equal("HOME", expanded[0].Title);
        Assert.False(vm.Menu.First(m => m.Page == typeof(InstalledViewModel)).ParentCategory!.IsExpanded);
        Assert.False(vm.Menu.First(m => m.Page == typeof(DiagnosticsViewModel)).ParentCategory!.IsExpanded);
        Assert.False(vm.Menu.First(m => m.Page == typeof(StudioViewModel)).ParentCategory!.IsExpanded);

        // Kapalı bölümlerin çocukları VisibleMenu'de yok.
        Assert.Contains(vm.Menu.First(m => m.Page == typeof(DashboardViewModel)), vm.VisibleMenu);
        Assert.DoesNotContain(vm.Menu.First(m => m.Page == typeof(InstalledViewModel)), vm.VisibleMenu);
    }

    [Fact]
    public async Task SelectingPage_ExpandsOnlyItsOwnSection_OthersStayExpanded()
    {
        using var host = Program.CreateHost();

        var vm = await Dispatcher.UIThread.InvokeAsync(() =>
            host.Services.GetRequiredService<MainViewModel>());

        var installed = vm.Menu.First(m => m.Page == typeof(InstalledViewModel));
        var diagnostics = vm.Menu.First(m => m.Page == typeof(DiagnosticsViewModel));
        var studio = vm.Menu.First(m => m.Page == typeof(StudioViewModel));

        var packages = installed.ParentCategory!;
        var system = diagnostics.ParentCategory!;
        var tools = studio.ParentCategory!;

        // Kullanıcı önce WORKFLOWS ve TOOLS'u açsın.
        var workflows = vm.Menu.First(m => m.Page == typeof(WorkspacesViewModel)).ParentCategory!;
        vm.ToggleGroupCommand.Execute(workflows);
        vm.ToggleGroupCommand.Execute(tools);

        Assert.True(workflows.IsExpanded);
        Assert.True(tools.IsExpanded);

        // PACKAGES'e git → yalnızca PACKAGES açılır; diğerleri kullanıcı durumunda kalır.
        vm.SelectedMenuItem = installed;

        Assert.True(packages.IsExpanded);
        Assert.True(workflows.IsExpanded);
        Assert.True(tools.IsExpanded);
        Assert.False(system.IsExpanded);

        // SYSTEM'e git → SYSTEM açılır; PACKAGES/WORKFLOWS/TOOLS kapanmaz.
        vm.SelectedMenuItem = diagnostics;

        Assert.True(system.IsExpanded);
        Assert.True(packages.IsExpanded);
        Assert.True(workflows.IsExpanded);
        Assert.True(tools.IsExpanded);

        // Tek seçim.
        Assert.Equal(1, vm.Menu.Count(m => m == vm.SelectedMenuItem));
    }

    [Fact]
    public async Task NavigatingBackToDashboard_DoesNotCollapseUserSections()
    {
        using var host = Program.CreateHost();

        var vm = await Dispatcher.UIThread.InvokeAsync(() =>
            host.Services.GetRequiredService<MainViewModel>());

        var dashboard = vm.Menu.First(m => m.Page == typeof(DashboardViewModel));
        var installed = vm.Menu.First(m => m.Page == typeof(InstalledViewModel));
        var system = vm.Menu.First(m => m.Page == typeof(DiagnosticsViewModel)).ParentCategory!;

        // Kullanıcı SYSTEM'i kapatmış, PACKAGES açık.
        Assert.False(system.IsExpanded);
        vm.SelectedMenuItem = installed;

        // Dashboard'a geri dön → HOME'i aç/kapat değişmez, SYSTEM kapalı kalır.
        vm.SelectedMenuItem = dashboard;

        Assert.True(installed.ParentCategory!.IsExpanded);
        Assert.False(system.IsExpanded);
        Assert.Equal(typeof(DashboardViewModel), vm.Navigation.CurrentPage?.GetType());
    }

    [Fact]
    public async Task RapidNavigation_NeverCollapsesAllSections_AndStaysStable()
    {
        using var host = Program.CreateHost();

        var vm = await Dispatcher.UIThread.InvokeAsync(() =>
            host.Services.GetRequiredService<MainViewModel>());

        // Kullanıcı WORKFLOWS + TOOLS'i açık bıraksın.
        var workflows = vm.Menu.First(m => m.Page == typeof(AutomationViewModel)).ParentCategory!;
        var tools = vm.Menu.First(m => m.Page == typeof(StudioViewModel)).ParentCategory!;
        vm.ToggleGroupCommand.Execute(workflows);
        vm.ToggleGroupCommand.Execute(tools);

        var targets = new[]
        {
            typeof(DashboardViewModel),
            typeof(InstalledViewModel),
            typeof(UpdatesViewModel),
            typeof(BrowseViewModel),
            typeof(DiagnosticsViewModel),
            typeof(SecurityViewModel),
            typeof(StudioViewModel),
            typeof(DeveloperViewModel),
            typeof(EnterpriseViewModel),
            typeof(GameModeViewModel),
            typeof(SettingsViewModel),
            typeof(DashboardViewModel),
            typeof(BrowseViewModel)
        };

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            for (var i = 0; i < 3; i++)
            foreach (var pageType in targets)
            {
                var item = vm.Menu.First(m => m.Page == pageType);
                vm.SelectedMenuItem = item;
            }
        });

        // State korunmuş: çoklu açık bölüm + tek seçim, bozulma yok.
        Assert.True(workflows.IsExpanded);
        Assert.True(tools.IsExpanded);
        Assert.Equal(1, vm.Menu.Count(m => m == vm.SelectedMenuItem));
        Assert.False(vm.SelectedMenuItem!.IsCategoryHeader);
        Assert.Contains(vm.SelectedMenuItem, vm.VisibleMenu);
    }

    [Fact]
    public async Task ToggleActiveSectionHeader_IsANoOp_ToKeepSelectionVisible()
    {
        using var host = Program.CreateHost();

        var vm = await Dispatcher.UIThread.InvokeAsync(() =>
            host.Services.GetRequiredService<MainViewModel>());

        var installed = vm.Menu.First(m => m.Page == typeof(InstalledViewModel));
        var packages = installed.ParentCategory!;

        vm.SelectedMenuItem = installed;
        Assert.True(packages.IsExpanded);

        // Kullanıcı aktif bölümün başlığına tıklasa bile seçim görünür kalır.
        vm.ToggleGroupCommand.Execute(packages);

        Assert.True(packages.IsExpanded);
        Assert.Contains(installed, vm.VisibleMenu);
        Assert.Equal(typeof(InstalledViewModel), vm.SelectedMenuItem?.Page);
    }
}