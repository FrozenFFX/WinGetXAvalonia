// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Core;

namespace WinGetXAvalonia.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public NavigationService Navigation { get; }

    public OperationDrawerViewModel OperationDrawer { get; }

    public string Version => "1.0.0";

    public string BuildDate =>
        new System.IO.FileInfo(typeof(MainViewModel).Assembly.Location)
            .LastWriteTime.ToString("yyyy-MM-dd");

    public string StatusText =>
        $"Ready • {DateTime.Now:T}";

    public ObservableCollection<NavigationItem> Menu { get; } = new();

    private readonly Dictionary<Type, ViewModelBase> _pages;

    [ObservableProperty]
    private NavigationItem? selectedMenuItem;

    public MainViewModel(
        NavigationService navigation,
        OperationDrawerViewModel operationDrawer,
        DashboardViewModel dashboard,
        InstalledViewModel installed,
        UpdatesViewModel updates,
        BrowseViewModel browse,
        HistoryViewModel history,
        WorkspacesViewModel workspaces,
        BlueprintsViewModel blueprints,
        AutomationViewModel automation,
        SecurityViewModel security,
        DiagnosticsViewModel diagnostics,
        SourceManagerViewModel sourceManager,
        StudioViewModel studio,
        DeveloperViewModel developer,
        SettingsViewModel settings)
    {
        Navigation = navigation;
        OperationDrawer = operationDrawer;

        // ViewModel tipinden örneğine eşleme, navigasyon bununla ilerliyor
        _pages = new()
        {
            [typeof(DashboardViewModel)] = dashboard,
            [typeof(InstalledViewModel)] = installed,
            [typeof(UpdatesViewModel)] = updates,
            [typeof(BrowseViewModel)] = browse,
            [typeof(HistoryViewModel)] = history,
            [typeof(WorkspacesViewModel)] = workspaces,
            [typeof(BlueprintsViewModel)] = blueprints,
            [typeof(AutomationViewModel)] = automation,
            [typeof(SecurityViewModel)] = security,
            [typeof(DiagnosticsViewModel)] = diagnostics,
            [typeof(SourceManagerViewModel)] = sourceManager,
            [typeof(StudioViewModel)] = studio,
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
            Title = "History",
            Icon = "🕘",
            Page = typeof(HistoryViewModel)
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
            Title = "Diagnostics",
            Icon = "🩺",
            Page = typeof(DiagnosticsViewModel)
        });

        Menu.Add(new()
        {
            Title = "Sources",
            Icon = "🌐",
            Page = typeof(SourceManagerViewModel)
        });

        Menu.Add(new()
        {
            Title = "Studio",
            Icon = "🎛",
            Page = typeof(StudioViewModel)
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

        // Installed ekranında liste boşken "Browse" denince Discover'a zıplatalım
        installed.BrowseRequested += (_, _) =>
        {
            var discover = Menu.FirstOrDefault(m => m.Page == typeof(BrowseViewModel));

            if (discover is not null)
                SelectedMenuItem = discover;
        };
    }

    partial void OnSelectedMenuItemChanged(NavigationItem? value)
    {
        if (value is null)
            return;

        if (!_pages.TryGetValue(value.Page, out var page))
            return;

        Navigation.Navigate(page);

        GC.Collect(
            GC.MaxGeneration,
            GCCollectionMode.Optimized,
            false,
            false);

        // sayfaları ilk ziyaret edildiğinde yüklüyoruz, sonra hazır bekliyor
        if (page is DashboardViewModel dashboard)
            _ = dashboard.InitializeAsync();
        else if (page is InstalledViewModel installed)
            _ = installed.InitializeAsync();
        else if (page is UpdatesViewModel updates)
            _ = updates.InitializeAsync();
        else if (page is BrowseViewModel browse)
            _ = browse.InitializeAsync();
        else if (page is HistoryViewModel history)
            _ = history.InitializeAsync();
        else if (page is WorkspacesViewModel workspaces)
            _ = workspaces.InitializeAsync();
        else if (page is StudioViewModel studio)
            _ = studio.InitializeAsync();
        else if (page is BlueprintsViewModel blueprints)
            _ = blueprints.InitializeAsync();
        else if (page is AutomationViewModel automation)
            _ = automation.InitializeAsync();
        else if (page is SecurityViewModel security)
            _ = security.InitializeAsync();
        else if (page is DiagnosticsViewModel diagnostics)
            _ = diagnostics.InitializeAsync();
        else if (page is SourceManagerViewModel sourceManager)
            _ = sourceManager.InitializeAsync();
    }
}
