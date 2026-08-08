// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Linq;
using Avalonia.Controls;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using WinGetXAvalonia.ViewModels;
using WinGetXAvalonia.Views;

namespace WinGetXAvalonia.Tests;

[Collection("UiDispatcher")]
public class SidebarClickNavigationTests
{    [Fact]
    public async Task ClickingSidebarCard_NavigatesToPage()
    {
        using var host = Program.CreateHost();

        var vm = await Dispatcher.UIThread.InvokeAsync(() =>
            host.Services.GetRequiredService<MainViewModel>());

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            vm.SelectedMenuItem = vm.Menu.First(m => m.Page == typeof(UpdatesViewModel));

            var window = new MainWindow { DataContext = vm };

            window.Width = 1600;
            window.Height = 900;

            window.Show();

            Dispatcher.UIThread.RunJobs();

            var mainHost = window.FindControl<ContentControl>("MainContentHost");

            Assert.NotNull(mainHost);
            Assert.Same(vm, mainHost.DataContext);
            Assert.IsType<UpdatesViewModel>(mainHost.Content);
            Assert.NotNull(mainHost.Presenter);
            Assert.IsType<UpdatesView>(mainHost.Presenter.Child);
        });
    }
}
