// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using Avalonia.Controls;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using WinGetXAvalonia.Controls.Layout;
using WinGetXAvalonia.ViewModels;
using WinGetXAvalonia.Views;

namespace WinGetXAvalonia.Tests;

[Collection("UiDispatcher")]
public class DetailPanelSmokeTests
{
    [Fact]
    public async Task ShowSystemDetails_OpensDetailHost_WithContent()
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

            var dashboard = host.Services.GetRequiredService<DashboardViewModel>();
            dashboard.ShowSystemDetailsCommand.Execute(null);

            Dispatcher.UIThread.RunJobs();

            var host_ = window.FindControl<DetailHost>("DetailHost");

            Assert.True(vm.IsDetailVisible);
            Assert.Equal("System", vm.DetailTitle);
            Assert.NotNull(vm.DetailContent);
            Assert.NotNull(host_);
            Assert.True(host_.IsVisible);

            Dispatcher.UIThread.RunJobs();

            Assert.NotNull(host_.Presenter);
            Assert.NotNull(host_.Presenter.Child);
            Assert.IsType<DetailListView>(host_.Presenter.Child);
        });
    }

    [Fact]
    public async Task CloseDetailCommand_HidesDetailHost()
    {
        using var host = Program.CreateHost();

        var vm = await Dispatcher.UIThread.InvokeAsync(() =>
            host.Services.GetRequiredService<MainViewModel>());

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            vm.ShowDetail(new DetailListViewModel(), "Title", null);
            Assert.True(vm.IsDetailVisible);

            vm.CloseDetailCommand.Execute(null);
            Assert.False(vm.IsDetailVisible);
        });
    }
}
