// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinGetXAvalonia.Core.State;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Core;

namespace WinGetXAvalonia.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public NavigationService Navigation { get; }

    public OperationDrawerViewModel OperationDrawer { get; }

    public CommandPaletteViewModel Palette { get; }

    private readonly LoggerService _logger;

    private readonly IErrorHandlingService _errorHandling;

    public string Version => "1.0.0";

    public string Footer =>
        $"WinGetX {Version} • {BuildDate}";

    public string BuildDate =>
        File.GetLastWriteTime(
            typeof(MainViewModel).Assembly.Location)
        .ToString("yyyy-MM-dd");

    public string StatusText =>
        SelectedMenuItem is null
            ? Status.ToString()
            : $"{Status} • {SelectedMenuItem.Title}";

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

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusText))]
    private AppStatus status = AppStatus.Ready;

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
        SettingsViewModel settings,
        LoggerService logger,
        IErrorHandlingService errorHandling)
    {
        Navigation = navigation;
        OperationDrawer = operationDrawer;
        Palette = commandPalette;
        _logger = logger;
        _errorHandling = errorHandling;

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
            Category = "Core"
        });

        Menu.Add(new()
        {
            Title = "Packages",
            Icon = "📦",
            Page = typeof(InstalledViewModel),
            Category = "Package Management"
        });

        Menu.Add(new()
        {
            Title = "Updates",
            Icon = "🔄",
            Page = typeof(UpdatesViewModel),
            Category = "Package Management"
        });

        Menu.Add(new()
        {
            Title = "History",
            Icon = "🕘",
            Page = typeof(HistoryViewModel),
            Category = "Package Management"
        });

        Menu.Add(new()
        {
            Title = "Discover",
            Icon = "🔍",
            Page = typeof(BrowseViewModel),
            Category = "Package Management"
        });

        Menu.Add(new()
        {
            Title = "Marketplace",
            Icon = "🛍️",
            Page = typeof(MarketplaceViewModel),
            Category = "Package Management"
        });

        Menu.Add(new()
        {
            Title = "Workspaces",
            Icon = "🗂",
            Page = typeof(WorkspacesViewModel),
            Category = "Workflows"
        });

        Menu.Add(new()
        {
            Title = "Blueprints",
            Icon = "📐",
            Page = typeof(BlueprintsViewModel),
            Category = "Workflows"
        });

        Menu.Add(new()
        {
            Title = "Automation",
            Icon = "⚙",
            Page = typeof(AutomationViewModel),
            Category = "Workflows"
        });

        Menu.Add(new()
        {
            Title = "Sources",
            Icon = "🌐",
            Page = typeof(SourceManagerViewModel),
            Category = "System"
        });

        Menu.Add(new()
        {
            Title = "Identity",
            Icon = "🪪",
            Page = typeof(SoftwareIdentityViewModel),
            Category = "System"
        });

        Menu.Add(new()
        {
            Title = "Security",
            Icon = "🛡",
            Page = typeof(SecurityViewModel),
            Category = "System"
        });

        Menu.Add(new()
        {
            Title = "Diagnostics",
            Icon = "🩺",
            Page = typeof(DiagnosticsViewModel),
            Category = "System"
        });

        Menu.Add(new()
        {
            Title = "Enterprise",
            Icon = "🏢",
            Page = typeof(EnterpriseViewModel),
            Category = "Enterprise"
        });

        Menu.Add(new()
        {
            Title = "Studio",
            Icon = "🎛",
            Page = typeof(StudioViewModel),
            Category = "Developer"
        });

        Menu.Add(new()
        {
            Title = "Developer",
            Icon = "🧑‍💻",
            Page = typeof(DeveloperViewModel),
            Category = "Developer"
        });

        Menu.Add(new()
        {
            Title = "Settings",
            Icon = "⚙",
            Page = typeof(SettingsViewModel),
            Category = "Developer"
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

        // Sayfalar ilk ziyaret edildiğinde yüklenir. Hata asla uygulamayı
        // çökertmez; log → history → notification → UI error state zincirinden
        // geçer (ARTICLE VII).
        _ = InitializePageSafelyAsync(page);
    }

    private async Task InitializePageSafelyAsync(ViewModelBase page)
    {
        Status = AppStatus.Loading;

        try
        {
            await page.InitializeAsync();
            Status = AppStatus.Ready;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Page initialization failed: {page.GetType().Name}");
            _errorHandling.HandlePackageOperationError(ex, $"initialize {page.GetType().Name}");
            Status = AppStatus.Error;

            if (page is AsyncViewModel asyncVm)
            {
                asyncVm.ErrorMessage = ex.Message;
                asyncVm.State = LoadState.Error;
            }
        }
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
            ("Core", "🎯"),
            ("Package Management", "📦"),
            ("Workflows", "🧩"),
            ("System", "🖥"),
            ("Enterprise", "🏢"),
            ("Developer", "🔧")
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
                    Title = catTitle.ToUpperInvariant(),
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
