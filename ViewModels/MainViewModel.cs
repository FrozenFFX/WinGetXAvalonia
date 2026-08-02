using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services;

namespace WinGetXAvalonia.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public NavigationService Navigation { get; }

    public ObservableCollection<NavigationItem> Menu { get; } = new();

    private readonly Dictionary<Type, ViewModelBase> _pages;

    [ObservableProperty]
    private NavigationItem? selectedMenuItem;

    public MainViewModel(
        NavigationService navigation,
        DashboardViewModel dashboard,
        InstalledViewModel installed,
        UpdatesViewModel updates,
        BrowseViewModel browse,
        FavoritesViewModel favorites,
        SettingsViewModel settings)
    {
        Navigation = navigation;

        _pages = new()
        {
            [typeof(DashboardViewModel)] = dashboard,
            [typeof(InstalledViewModel)] = installed,
            [typeof(UpdatesViewModel)] = updates,
            [typeof(BrowseViewModel)] = browse,
            [typeof(FavoritesViewModel)] = favorites,
            [typeof(SettingsViewModel)] = settings
        };

        Menu.Add(new()
        {
            Title = "Dashboard",
            Icon = "🏠",
            Page = typeof(DashboardViewModel)
        });

        Menu.Add(new()
        {
            Title = "Installed",
            Icon = "📦",
            Page = typeof(InstalledViewModel)
        });

        Menu.Add(new()
        {
            Title = "Updates",
            Icon = "🔄",
            Page = typeof(UpdatesViewModel)
        });

        Menu.Add(new()
        {
            Title = "Browse",
            Icon = "🔍",
            Page = typeof(BrowseViewModel)
        });

        Menu.Add(new()
        {
            Title = "Favorites",
            Icon = "⭐",
            Page = typeof(FavoritesViewModel)
        });

        Menu.Add(new()
        {
            Title = "Settings",
            Icon = "⚙",
            Page = typeof(SettingsViewModel)
        });

        SelectedMenuItem = Menu[0];
    }

    partial void OnSelectedMenuItemChanged(NavigationItem? value)
    {
        if (value is null)
            return;

        if (!_pages.TryGetValue(value.Page, out var page))
            return;

        Navigation.Navigate(page);

        if (page is DashboardViewModel dashboard)
            _ = dashboard.InitializeAsync();
    }
}
