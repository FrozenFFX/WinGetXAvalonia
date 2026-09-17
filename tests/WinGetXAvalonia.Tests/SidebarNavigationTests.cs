// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using WinGetXAvalonia.ViewModels;

namespace WinGetXAvalonia.Tests;

[Collection("UiDispatcher")]
public class SidebarNavigationTests
{
    [Fact]
    public async Task ViewModelNavigation_ChangesCurrentPage()
    {
        using var host = Program.CreateHost();

        var vm = await Dispatcher.UIThread.InvokeAsync(() =>
            host.Services.GetRequiredService<MainViewModel>());

        var navigation = vm.Navigation;

        Assert.Equal(typeof(DashboardViewModel), navigation.CurrentPage?.GetType());

        var packages = vm.Menu.First(m => m.Page == typeof(InstalledViewModel));

        await Dispatcher.UIThread.InvokeAsync(() => vm.SelectedMenuItem = packages);

        Assert.Equal(typeof(InstalledViewModel), navigation.CurrentPage?.GetType());
    }

    [Fact]
    public async Task SidebarNavigation_UpdatesSelectedMenuItem()
    {
        using var host = Program.CreateHost();

        var vm = await Dispatcher.UIThread.InvokeAsync(() =>
            host.Services.GetRequiredService<MainViewModel>());

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            var window = new Window { DataContext = vm };

            var sidebar = new WinGetXAvalonia.Controls.Layout.LeftSidebar();
            sidebar.DataContext = vm;
            window.Content = sidebar;

            window.Show();
            Dispatcher.UIThread.RunJobs();

            // Bölümler kapalı başlar; kullanıcının PACKAGES'ı açtığını simüle edelim.
            var packages = vm.Menu.First(m => m.Page == typeof(InstalledViewModel));
            vm.ToggleGroupCommand.Execute(packages.ParentCategory);

            Dispatcher.UIThread.RunJobs();

            var listBox = FindListBox(window);

            Assert.NotNull(listBox);

            listBox.SelectedItem = vm.Menu.First(m => m.Page == typeof(UpdatesViewModel));

            Dispatcher.UIThread.RunJobs();

            Assert.Equal(typeof(UpdatesViewModel), vm.SelectedMenuItem?.Page);
            Assert.Equal(typeof(UpdatesViewModel), vm.Navigation.CurrentPage?.GetType());
        });
    }

    private static ListBox? FindListBox(Avalonia.Visual root)
    {
        if (root is ListBox listBox)
            return listBox;

        foreach (var child in root.GetVisualChildren())
        {
            if (FindListBox(child) is { } found)
                return found;
        }

        return null;
    }
}
