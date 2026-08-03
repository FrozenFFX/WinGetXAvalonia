// ==========================================================
// WinGetX - https://github.com/FrozenFFX
// ==========================================================
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Core;

namespace WinGetXAvalonia.ViewModels;

// kaneki
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
        WorkspacesViewModel workspaces,
        BlueprintsViewModel blueprints,
        AutomationViewModel automation,
        SecurityViewModel security,
        DeveloperViewModel developer,
        SettingsViewModel settings)
    {
        Navigation = navigation;

        _pages = new()
        {
            [typeof(DashboardViewModel)] = dashboard,
            [typeof(InstalledViewModel)] = installed,
            [typeof(UpdatesViewModel)] = updates,
            [typeof(BrowseViewModel)] = browse,
            [typeof(WorkspacesViewModel)] = workspaces,
            [typeof(BlueprintsViewModel)] = blueprints,
            [typeof(AutomationViewModel)] = automation,
            [typeof(SecurityViewModel)] = security,
            [typeof(DeveloperViewModel)] = developer,
            [typeof(SettingsViewModel)] = settings
        };

        Menu.Add(new()
        {
            Title = "Home",
            Icon = "🏠",
            Page = typeof(DashboardViewModel)
        });

        Menu.Add(new()
        {
            Title = "Packages",
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
            Title = "Discover",
            Icon = "🔍",
            Page = typeof(BrowseViewModel)
        });

        Menu.Add(new()
        {
            Title = "Workspaces",
            Icon = "🗂",
            Page = typeof(WorkspacesViewModel)
        });

        Menu.Add(new()
        {
            Title = "Blueprints",
            Icon = "📐",
            Page = typeof(BlueprintsViewModel)
        });

        Menu.Add(new()
        {
            Title = "Automation",
            Icon = "⚙",
            Page = typeof(AutomationViewModel)
        });

        Menu.Add(new()
        {
            Title = "Security",
            Icon = "🛡",
            Page = typeof(SecurityViewModel)
        });

        Menu.Add(new()
        {
            Title = "Developer",
            Icon = "🧑‍💻",
            Page = typeof(DeveloperViewModel)
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
        else if (page is InstalledViewModel installed)
            _ = installed.InitializeAsync();
        else if (page is BrowseViewModel browse)
            _ = browse.InitializeAsync();
    }
}
// kaneki
