// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using WinGetXAvalonia.ViewModels;
using WinGetXAvalonia.Views;

namespace WinGetXAvalonia.Tests;

[Collection("UiDispatcher")]
public class DiagnosticsProbeTests
{
    [Fact]
    public async Task Startup_Dashboard_Renders()
    {
        using var host = Program.CreateHost();

        var vm = await Dispatcher.UIThread.InvokeAsync(() =>
            host.Services.GetRequiredService<MainViewModel>());

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            var window = new MainWindow { DataContext = vm };
            window.Width = 1600;
            window.Height = 900;
            window.Show();

            Dispatcher.UIThread.RunJobs();

            var mainHost = window.FindControl<ContentControl>("MainContentHost");

            Assert.NotNull(mainHost?.Presenter?.Child);
            Assert.IsType<DashboardView>(mainHost!.Presenter!.Child);
        });
    }

    [Fact]
    public async Task Management_Header_TogglesGroup_WithoutNavigating()
    {
        using var host = Program.CreateHost();

        var vm = await Dispatcher.UIThread.InvokeAsync(() =>
            host.Services.GetRequiredService<MainViewModel>());

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            var header = vm.Menu.FirstOrDefault(m => m.IsCategoryHeader && m.Category == "Package Management");
            var packages = vm.Menu.FirstOrDefault(m => m.Page == typeof(InstalledViewModel));
            var updates = vm.Menu.FirstOrDefault(m => m.Page == typeof(UpdatesViewModel));

            Assert.NotNull(header);
            Assert.NotNull(packages);
            Assert.NotNull(updates);
            Assert.Same(header, packages.ParentCategory);
            Assert.Same(header, updates.ParentCategory);
            Assert.True(header.IsExpanded);

            vm.SelectedMenuItem = packages;
            Assert.IsType<InstalledViewModel>(vm.Navigation.CurrentPage);

            // Başlık tıklanınca sayfa değişmemeli; sadece grup kapanmalı.
            vm.SelectedMenuItem = header;

            Assert.False(header.IsExpanded);
            Assert.Equal("Packages", vm.SelectedMenuItem?.Title);
            Assert.IsType<InstalledViewModel>(vm.Navigation.CurrentPage);
            Assert.DoesNotContain(packages, vm.VisibleMenu);
            Assert.DoesNotContain(updates, vm.VisibleMenu);
            Assert.Contains(header, vm.VisibleMenu);

            // Tekrar tıklanınca grup açılmalı.
            vm.SelectedMenuItem = header;

            Assert.True(header.IsExpanded);
            Assert.Equal("Packages", vm.SelectedMenuItem?.Title);
            Assert.IsType<InstalledViewModel>(vm.Navigation.CurrentPage);
        });
    }

    [Fact]
    public async Task NavigationItem_IsVisible_FollowsParentExpansion()
    {
        var header = new WinGetXAvalonia.Models.NavigationItem
        {
            IsCategoryHeader = true,
            Title = "Management"
        };

        var child = new WinGetXAvalonia.Models.NavigationItem
        {
            Title = "Packages"
        };

        header.AddChild(child);
        child.ParentCategory = header;

        Assert.True(child.IsVisible);

        header.IsExpanded = false;
        Assert.False(child.IsVisible);

        header.IsExpanded = true;
        Assert.True(child.IsVisible);
    }

    [Fact]
    public async Task StandaloneItem_IsAlwaysVisible()
    {
        var item = new WinGetXAvalonia.Models.NavigationItem { Title = "Dashboard" };

        Assert.True(item.IsVisible);
    }
}
