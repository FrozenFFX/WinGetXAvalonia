// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Core;

namespace WinGetXAvalonia.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public NavigationService Navigation { get; }

    public OperationDrawerViewModel OperationDrawer { get; }

    public CommandPaletteViewModel Palette { get; }

    public string Version => "1.0.0";

    public string Footer =>
        $"WinGetX {Version} • {BuildDate}";

    public string BuildDate =>
        File.GetLastWriteTime(
            typeof(MainViewModel).Assembly.Location)
        .ToString("yyyy-MM-dd");

    public string StatusText =>
        SelectedMenuItem is null
            ? "Ready"
            : $"Ready • {SelectedMenuItem.Title}";

    public ObservableCollection<NavigationItem> Menu { get; } = new();

    // Sidebar'ın bağlandığı görünür menü. Daraltılmış kategorilerin öğelerini
    // içermez — ListBox sanallaştırması görünmez öğelerle bozulmasın diye
    // IsVisible yerine koleksiyondan filtreleriz.
    public ObservableCollection<NavigationItem> VisibleMenu { get; } = new();

    public ObservableCollection<NavigationCategory> MenuCategories { get; } = new();

    private readonly Dictionary<Type, ViewModelBase> _pages;

    private NavigationItem? _lastPageItem;

    private bool _restoringSelection;

    [ObservableProperty]
    private NavigationItem? selectedMenuItem;

    [ObservableProperty]
    private bool isDetailVisible;

    [ObservableProperty]
    private string detailTitle = "";

    [ObservableProperty]
    private string? detailSubtitle;

    [ObservableProperty]
    private object? detailContent;

    public ICommand CloseDetailCommand { get; }

    public MainViewModel(
        NavigationService navigation,
        OperationDrawerViewModel operationDrawer,
        CommandPaletteViewModel commandPalette,
        DashboardViewModel dashboard,
        InstalledViewModel installed,
        UpdatesViewModel updates,
        BrowseViewModel browse,
        HistoryViewModel history,
        WorkspacesViewModel workspaces,
        BlueprintsViewModel blueprints,
        MarketplaceViewModel marketplace,
        EnterpriseViewModel enterprise,
        SoftwareIdentityViewModel softwareIdentity,
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
        Palette = commandPalette;

        CloseDetailCommand = new RelayCommand(() => IsDetailVisible = false);

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
            [typeof(MarketplaceViewModel)] = marketplace,
            [typeof(EnterpriseViewModel)] = enterprise,
            [typeof(SoftwareIdentityViewModel)] = softwareIdentity,
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
            Title = "Dashboard",
            Icon = "📊",
            Page = typeof(DashboardViewModel),
            Category = "Dashboard"
        });

        Menu.Add(new()
        {
            Title = "Packages",
            Icon = "📦",
            Page = typeof(InstalledViewModel),
            Category = "Management"
        });

        Menu.Add(new()
        {
            Title = "Updates",
            Icon = "🔄",
            Page = typeof(UpdatesViewModel),
            Category = "Management"
        });

        Menu.Add(new()
        {
            Title = "History",
            Icon = "🕘",
            Page = typeof(HistoryViewModel),
            Category = "Management"
        });

        Menu.Add(new()
        {
            Title = "Discover",
            Icon = "🔍",
            Page = typeof(BrowseViewModel),
            Category = "Discover"
        });

        Menu.Add(new()
        {
            Title = "Marketplace",
            Icon = "🛍️",
            Page = typeof(MarketplaceViewModel),
            Category = "Discover"
        });

        Menu.Add(new()
        {
            Title = "Workspaces",
            Icon = "🗂",
            Page = typeof(WorkspacesViewModel),
            Category = "Workspaces"
        });

        Menu.Add(new()
        {
            Title = "Blueprints",
            Icon = "📐",
            Page = typeof(BlueprintsViewModel),
            Category = "Workspaces"
        });

        Menu.Add(new()
        {
            Title = "Automation",
            Icon = "⚙",
            Page = typeof(AutomationViewModel),
            Category = "Automation"
        });

        Menu.Add(new()
        {
            Title = "Sources",
            Icon = "🌐",
            Page = typeof(SourceManagerViewModel),
            Category = "Configuration"
        });

        Menu.Add(new()
        {
            Title = "Identity",
            Icon = "🪪",
            Page = typeof(SoftwareIdentityViewModel),
            Category = "Security"
        });

        Menu.Add(new()
        {
            Title = "Security",
            Icon = "🛡",
            Page = typeof(SecurityViewModel),
            Category = "Security"
        });

        Menu.Add(new()
        {
            Title = "Diagnostics",
            Icon = "🩺",
            Page = typeof(DiagnosticsViewModel),
            Category = "Security"
        });

        Menu.Add(new()
        {
            Title = "Enterprise",
            Icon = "🏢",
            Page = typeof(EnterpriseViewModel),
            Category = "Security"
        });

        Menu.Add(new()
        {
            Title = "Studio",
            Icon = "🎛",
            Page = typeof(StudioViewModel),
            Category = "Tools"
        });

        Menu.Add(new()
        {
            Title = "Developer",
            Icon = "🧑‍💻",
            Page = typeof(DeveloperViewModel),
            Category = "Tools"
        });

        Menu.Add(new()
        {
            Title = "Settings",
            Icon = "⚙",
            Page = typeof(SettingsViewModel),
            Category = "Tools"
        });

        BuildMenuGrouped();
        RebuildVisibleMenu();

        SelectedMenuItem = Menu.FirstOrDefault(m => !m.IsCategoryHeader);

        // Command palette, sidebar seçimini senkron tutsun
        Palette.NavigateToPage = pageType =>
        {
            var item = Menu.FirstOrDefault(m => m.Page == pageType);

            if (item is null)
                return false;

            SelectedMenuItem = item;

            return true;
        };

        Palette.SearchPackages = query =>
        {
            var discover = Menu.FirstOrDefault(m => m.Page == typeof(BrowseViewModel));

            if (discover is not null)
                SelectedMenuItem = discover;

            browse.SearchText = query;
            browse.SearchCommand.Execute(null);
        };

        // Installed ekranında liste boşken "Browse" denince Discover'a zıplatalım
        installed.BrowseRequested += (_, _) =>
        {
            var discover = Menu.FirstOrDefault(m => m.Page == typeof(BrowseViewModel));

            if (discover is not null)
                SelectedMenuItem = discover;
        };

        // Dashboard quick action "Search" → komut paletini aç
        dashboard.SearchRequested += () => Palette.Open();

        // Dashboard detay kartları → sağ paneli aç
        dashboard.DetailRequested += (content, title, subtitle) =>
            ShowDetail(content, title, subtitle);

        Navigation.Navigate(_pages[typeof(DashboardViewModel)]);
    }

    partial void OnSelectedMenuItemChanged(NavigationItem? value)
    {
        if (value is null)
            return;

        // Kategori başlığı (Management, Configuration, Tools vb.) tıklanınca
        // sayfaya gitmez; grubu açıp kapatır ve seçimi son sayfada tutar.
        if (value.IsCategoryHeader)
        {
            value.IsExpanded = !value.IsExpanded;
            RebuildVisibleMenu();

            if (_lastPageItem is not null)
            {
                _restoringSelection = true;
                SelectedMenuItem = _lastPageItem;
                _restoringSelection = false;
            }

            return;
        }

        _lastPageItem = value;

        if (_restoringSelection)
            return;

        if (!_pages.TryGetValue(value.Page, out var page))
            return;

        if (!Navigation.Navigate(page))
            return;

        OnPropertyChanged(nameof(StatusText));

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
        else if (page is MarketplaceViewModel marketplace)
            _ = marketplace.InitializeAsync();
        else if (page is EnterpriseViewModel enterprise)
            _ = enterprise.InitializeAsync();
        else if (page is SoftwareIdentityViewModel softwareIdentity)
            _ = softwareIdentity.InitializeAsync();
        else if (page is AutomationViewModel automation)
            _ = automation.InitializeAsync();
        else if (page is SecurityViewModel security)
            _ = security.InitializeAsync();
        else if (page is DiagnosticsViewModel diagnostics)
            _ = diagnostics.InitializeAsync();
        else if (page is SourceManagerViewModel sourceManager)
            _ = sourceManager.InitializeAsync();
    }

    public void RefreshStatus()
    {
        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(Footer));

        foreach (var cat in MenuCategories)
            cat.IsExpanded = true;

        UpdateBadgeCounts();
    }

    private void UpdateBadgeCounts()
    {
        var updatesItem = Menu.FirstOrDefault(m => m.Page == typeof(UpdatesViewModel));
        if (updatesItem is not null)
            updatesItem.BadgeCount = _pages.Values
                .OfType<UpdatesViewModel>()
                .FirstOrDefault()?.PackageCount ?? 0;
    }

    private void BuildMenuGrouped()
    {
        var categoryDefs = new (string Title, string Icon)[]
        {
            ("Dashboard", "📊"),
            ("Management", "📦"),
            ("Discover", "🔍"),
            ("Workspaces", "🗂"),
            ("Automation", "⚙"),
            ("Configuration", "🌐"),
            ("Security", "🛡"),
            ("Tools", "🔧")
        };

        var grouped = new List<NavigationItem>();

        foreach (var (catTitle, catIcon) in categoryDefs)
        {
            var items = Menu.Where(m => m.Category == catTitle).ToArray();

            if (!items.Any())
                continue;

            NavigationItem? header = null;

            // Kategori başlığı, altındaki bir item'la aynı adı taşıyorsa
            // başlığı atla — menüde "Discover / Discover" gibi çift görünüm olmasın.
            if (!items.Any(m => string.Equals(
                    m.Title, catTitle, StringComparison.OrdinalIgnoreCase)))
            {
                header = new NavigationItem
                {
                    Title = catTitle,
                    Icon = catIcon,
                    Category = catTitle,
                    IsCategoryHeader = true
                };

                grouped.Add(header);
            }

            foreach (var item in items)
            {
                if (header is not null)
                {
                    item.ParentCategory = header;
                    header.AddChild(item);
                }

                grouped.Add(item);
            }
        }

        Menu.Clear();

        foreach (var item in grouped)
            Menu.Add(item);
    }

    private void RebuildVisibleMenu()
    {
        VisibleMenu.Clear();

        foreach (var item in Menu)
        {
            if (item.IsCategoryHeader || item.IsVisible)
                VisibleMenu.Add(item);
        }
    }

    public void ShowDetail(object content, string title, string? subtitle = null)
    {
        DetailContent = content;
        DetailTitle = title;
        DetailSubtitle = subtitle;
        IsDetailVisible = true;
    }
}
