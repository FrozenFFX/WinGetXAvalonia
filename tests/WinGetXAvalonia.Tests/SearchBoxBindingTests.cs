// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using WinGetXAvalonia.Controls.Shared;
using WinGetXAvalonia.ViewModels;
using WinGetXAvalonia.Views;

namespace WinGetXAvalonia.Tests;

[Collection("UiDispatcher")]
public partial class SearchBoxBindingTests
{
    private sealed partial class ProbeViewModel : ObservableObject
    {
        [ObservableProperty]
        private string searchText = "";
    }

    [Fact]
    public async Task Typing_InSearchBox_UpdatesViewModel()
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            var vm = new ProbeViewModel();

            var box = new SearchBox
            {
                Watermark = "Search...",
                DataContext = vm,
            };

            box.Bind(SearchBox.TextProperty, new Binding("SearchText") { Mode = BindingMode.TwoWay });

            var window = new Window
            {
                Content = box,
                DataContext = vm,
                Width = 600,
                Height = 200,
            };

            window.Show();

            Dispatcher.UIThread.RunJobs();

            var field = box.FindDescendantOfType<TextBox>();

            Assert.NotNull(field);

            field!.Text = "gaming";

            Dispatcher.UIThread.RunJobs();

            Assert.Equal("gaming", vm.SearchText);
        });
    }

    [Fact]
    public async Task ClearButton_EmptiesViewModel()
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            var vm = new ProbeViewModel();

            var box = new SearchBox
            {
                Watermark = "Search...",
                DataContext = vm,
            };

            box.Bind(SearchBox.TextProperty, new Binding("SearchText") { Mode = BindingMode.TwoWay });

            var window = new Window
            {
                Content = box,
                DataContext = vm,
                Width = 600,
                Height = 200,
            };

            window.Show();

            Dispatcher.UIThread.RunJobs();

            box.Text = "hello";

            Dispatcher.UIThread.RunJobs();

            Assert.Equal("hello", vm.SearchText);

            var clear = box.FindDescendantOfType<AppButton>();

            Assert.NotNull(clear);

            clear!.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            Dispatcher.UIThread.RunJobs();

            Assert.Equal("", vm.SearchText);
        });
    }

    [Fact]
    public async Task Typing_InRealMarketplaceView_FiltersItems()
    {
        using var host = Program.CreateHost();

        var vm = await Dispatcher.UIThread.InvokeAsync(() =>
            host.Services.GetRequiredService<MarketplaceViewModel>());

        await vm.LoadAsync();

        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var view = new MarketplaceView { DataContext = vm };

            var window = new Window
            {
                Content = view,
                DataContext = vm,
                Width = 1400,
                Height = 900,
            };

            window.Show();

            Dispatcher.UIThread.RunJobs();

            var field = view.FindDescendantOfType<TextBox>()
                ?? throw new Xunit.Sdk.XunitException("Search TextBox not found in MarketplaceView");

            Assert.Equal(10, vm.Items.Count);

            field.Text = "gaming";

            Dispatcher.UIThread.RunJobs();

            Assert.Equal("gaming", vm.SearchText);
            Assert.Single(vm.Items);
            Assert.Contains("Gaming Essentials", vm.Items[0].Name);
        });
    }
}
