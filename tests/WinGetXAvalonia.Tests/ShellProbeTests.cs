// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using WinGetXAvalonia;
using WinGetXAvalonia.Controls.Shared;
using WinGetXAvalonia.ViewModels;
using WinGetXAvalonia.Views;

namespace WinGetXAvalonia.Tests;

// Tüm sekmeleri dolaşır; navigasyon hattını ve her sayfanın render olup
// içindeki her TextBox'un aktif (enabled) ve metin kabul ettiğini doğrular.
[Collection("UiDispatcher")]
public class ShellProbeTests
{
    [Fact]
    public async Task MainWindow_AdaptsNavigationToNarrowWindows()
    {
        using var host = Program.CreateHost();
        var viewModel = await Dispatcher.UIThread.InvokeAsync(() =>
            host.Services.GetRequiredService<MainViewModel>());

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            var window = new MainWindow
            {
                DataContext = viewModel,
                Width = 390,
                Height = 700,
                WindowState = WindowState.Normal
            };
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var menuButton = window.FindControl<Button>("MobileMenuButton");
            var sidebar = window.FindControl<Control>("DesktopSidebar");
            var scrim = window.FindControl<Control>("MobileScrim");
            var content = window.FindControl<ScrollViewer>("MainContentScroller");
            var shell = window.FindControl<Grid>("ShellRoot");

            Assert.NotNull(menuButton);
            Assert.NotNull(sidebar);
            Assert.NotNull(scrim);
            Assert.NotNull(content);
            Assert.NotNull(shell);
            Assert.True(menuButton.IsVisible);
            Assert.False(sidebar.IsVisible);
            Assert.Equal(0, Grid.GetColumn(content));
            Assert.Equal(4, Grid.GetColumnSpan(content));

            menuButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Dispatcher.UIThread.RunJobs();
            Assert.True(sidebar.IsVisible);
            Assert.True(scrim.IsVisible);

            viewModel.SelectedMenuItem = viewModel.Menu.First(item =>
                !item.IsCategoryHeader && item != viewModel.SelectedMenuItem);
            Dispatcher.UIThread.RunJobs();
            Assert.False(sidebar.IsVisible);
            Assert.False(scrim.IsVisible);

            window.Width = 1600;
            Dispatcher.UIThread.RunJobs();
            Assert.False(menuButton.IsVisible);
            Assert.True(sidebar.IsVisible);
            Assert.Equal(1, Grid.GetColumn(content));
            Assert.Equal(1, Grid.GetColumnSpan(content));

            window.Close();
        });
    }

    [Fact]
    public async Task EveryPage_Navigates_And_TextboxesAreActive()
    {
        using var host = Program.CreateHost();

        var vm = await Dispatcher.UIThread.InvokeAsync(() =>
            host.Services.GetRequiredService<MainViewModel>());

        var pages = vm.Menu.Where(m => !m.IsCategoryHeader).ToArray();

        Assert.NotEmpty(pages);

        var failures = new List<string>();

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            var window = new MainWindow
            {
                DataContext = vm,
                Width = 1600,
                Height = 900
            };

            window.Show();
            Dispatcher.UIThread.RunJobs();

            var viewLocator = new ViewLocator();

            foreach (var item in pages)
            {
                // 1) Navigasyon hattı: Content (ViewModel) anında güncellenir.
                //    View swap'i 150ms geçiş animasyonuyla yapılır, o yüzden
                //    Presenter.Child yerine Content doğrulanır.
                vm.SelectedMenuItem = item;
                Dispatcher.UIThread.RunJobs();

                var mainHost = window.FindControl<ContentControl>("MainContentHost");

                if (mainHost?.Content is not ViewModelBase current ||
                    current.GetType() != item.Page)
                {
                    failures.Add(
                        $"{item.Title}: navigasyon Content " +
                        $"'{mainHost?.Content?.GetType().Name ?? "null"}'");
                }

                // 2) View + TextBox kontrolü — geçiş animasyonundan bağımsız.
                var pageVm = host.Services.GetRequiredService(item.Page) as ViewModelBase;

                if (pageVm is null || viewLocator.Build(pageVm) is not Control view)
                {
                    failures.Add($"{item.Title}: view bulunamadı");
                    continue;
                }

                var viewType = ResolveViewType(item.Page);

                if (viewType is not null && !viewType.IsAssignableFrom(view.GetType()))
                {
                    failures.Add(
                        $"{item.Title}: beklenen {viewType.Name} fakat {view.GetType().Name}");
                }

                var probe = new Window
                {
                    Content = view,
                    DataContext = pageVm,
                    Width = 1400,
                    Height = 900
                };

                probe.Show();
                Dispatcher.UIThread.RunJobs();

                var boxes = view.GetVisualDescendants()
                    .OfType<TextBox>()
                    .ToList();

                foreach (var box in boxes)
                {
                    if (!box.IsEnabled)
                        failures.Add($"{item.Title}: disabled TextBox '{box.Name}'");

                    var original = box.Text;
                    box.Text = "__probe__";
                    Dispatcher.UIThread.RunJobs();

                    if (box.Text != "__probe__")
                        failures.Add($"{item.Title}: TextBox '{box.Name}' metin kabul etmiyor");

                    box.Text = original;
                }

                probe.Width = 390;
                probe.Height = 840;
                Dispatcher.UIThread.RunJobs();

                if (probe.Bounds.Width >= 760)
                    failures.Add($"{item.Title}: dar pencere boyutu uygulanmadı");

                if (view is DashboardView dashboard && !dashboard.IsCompactLayout)
                    failures.Add($"{item.Title}: pano dar ekran düzenine geçmedi");

                if (view is BrowseView browse && !browse.IsCompactLayout)
                    failures.Add($"{item.Title}: paket keşfi dar ekran düzenine geçmedi");

                if (view.GetVisualDescendants().OfType<MasterDetailPage>()
                    .Any(masterDetail => !masterDetail.IsCompactLayout))
                {
                    failures.Add($"{item.Title}: ayrıntı paneli dar ekran düzenine geçmedi");
                }

                Dispatcher.UIThread.RunJobs();
                probe.Close();
            }

            window.Close();
        });

        Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures));
    }

    private static Type? ResolveViewType(Type viewModelType)
    {
        var ns = viewModelType.Namespace?
            .Replace("ViewModels", "Views", System.StringComparison.Ordinal);

        var name = viewModelType.Name
            .Replace("ViewModel", "View", System.StringComparison.Ordinal);

        return viewModelType.Assembly.GetType($"{ns}.{name}");
    }
}
