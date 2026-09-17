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
public class BrowseNavigationTests
{
    // Regression: BrowseView kategorileri, ItemsControl item template'indeki
    // "Data" binding'i için StaticResource "IconGeometry" kullanıyordu; kayıtlı
    // olmayan "IconGeometryConverter" anahtarına referans verilince sayfa ilk
    // ölçülürken KeyNotFoundException fırlatılıp uygulama kapanıyordu.
    [Fact]
    public async Task NavigatingToBrowse_BuildsViewAndDeferredTemplates_WithoutCrashing()
    {
        using var host = Program.CreateHost();

        var vm = await Dispatcher.UIThread.InvokeAsync(() =>
            host.Services.GetRequiredService<MainViewModel>());

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            vm.SelectedMenuItem = vm.Menu.First(m => m.Page == typeof(BrowseViewModel));

            var window = new MainWindow { DataContext = vm };
            window.Width = 1600;
            window.Height = 900;

            window.Show();

            Dispatcher.UIThread.RunJobs();

            var mainHost = window.FindControl<ContentControl>("MainContentHost");

            Assert.NotNull(mainHost);
            Assert.IsType<BrowseView>(mainHost.Presenter?.Child);
        });
    }
}