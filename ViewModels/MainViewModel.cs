// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinGetXAvalonia.Core.State;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Core;
using WinGetXAvalonia.Services.Updates;

namespace WinGetXAvalonia.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public NavigationService Navigation { get; }

    public OperationDrawerViewModel OperationDrawer { get; }

    public CommandPaletteViewModel Palette { get; }

    private readonly LoggerService _logger;

    private readonly IErrorHandlingService _errorHandling;

    private readonly IUpdateService _updateService;

    public string Version =>
        Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion.Split('+')[0]
            ?? "1.0.0";

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

    public string StatusLabel => Status.ToString();

    public ObservableCollection<NavigationItem> Menu { get; } = new();

    // Görünür menü; daraltılmış kategorilerin öğelerini içermez
    // (ListBox sanallaştırması görünmez öğelerle bozulmamasın diye).
    public ObservableCollection<NavigationItem> VisibleMenu { get; } = new();

    private readonly Dictionary<Type, ViewModelBase> _pages;

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
    [NotifyPropertyChangedFor(nameof(StatusLabel))]
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
        FavoritesViewModel favorites,
        GameModeViewModel gameMode,
        IUpdateService updateService,
        LoggerService logger,
        IErrorHandlingService errorHandling)
    {
        Navigation = navigation;
        OperationDrawer = operationDrawer;
        Palette = commandPalette;
        _logger = logger;
        _errorHandling = errorHandling;
        _updateService = updateService;

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
            [typeof(SettingsViewModel)] = settings,
            [typeof(FavoritesViewModel)] = favorites,
            [typeof(GameModeViewModel)] = gameMode
        };

        Menu.Add(new()
        {
            Title = "Dashboard",
            Icon = "home",
            Page = typeof(DashboardViewModel),
            Category = "Home"
        });

        Menu.Add(new()
        {
            Title = "Installed",
            Icon = "package",
            Page = typeof(InstalledViewModel),
            Category = "Packages"
        });

        Menu.Add(new()
        {
            Title = "Updates",
            Icon = "update",
            Page = typeof(UpdatesViewModel),
            Category = "Packages"
        });

        Menu.Add(new()
        {
            Title = "Browse",
            Icon = "search",
            Page = typeof(BrowseViewModel),
            Category = "Packages"
        });

        Menu.Add(new()
        {
            Title = "Sources",
            Icon = "source",
            Page = typeof(SourceManagerViewModel),
            Category = "Packages"
        });

        Menu.Add(new()
        {
            Title = "History",
            Icon = "history",
            Page = typeof(HistoryViewModel),
            Category = "Packages"
        });

        Menu.Add(new()
        {
            Title = "Favorites",
            Icon = "favorite",
            Page = typeof(FavoritesViewModel),
            Category = "Packages"
        });

        Menu.Add(new()
        {
            Title = "Queue",
            Icon = "queue",
            Page = typeof(WorkspacesViewModel),
            Category = "Workflows"
        });

        Menu.Add(new()
        {
            Title = "Automation",
            Icon = "automation",
            Page = typeof(AutomationViewModel),
            Category = "Workflows"
        });

        Menu.Add(new()
        {
            Title = "Blueprints",
            Icon = "collection",
            Page = typeof(BlueprintsViewModel),
            Category = "Workflows"
        });

        Menu.Add(new()
        {
            Title = "Diagnostics",
            Icon = "diagnostics",
            Page = typeof(DiagnosticsViewModel),
            Category = "System"
        });

        Menu.Add(new()
        {
            Title = "Security",
            Icon = "security",
            Page = typeof(SecurityViewModel),
            Category = "System"
        });

        Menu.Add(new()
        {
            Title = "Studio",
            Icon = "studio",
            Page = typeof(StudioViewModel),
            Category = "Tools"
        });

        Menu.Add(new()
        {
            Title = "Developer",
            Icon = "developer",
            Page = typeof(DeveloperViewModel),
            Category = "Tools"
        });

        Menu.Add(new()
        {
            Title = "Enterprise",
            Icon = "enterprise",
            Page = typeof(EnterpriseViewModel),
            Category = "Tools"
        });

        Menu.Add(new()
        {
            Title = "Settings",
            Icon = "settings",
            Page = typeof(SettingsViewModel),
            Category = "Settings"
        });

        Menu.Add(new()
        {
            Title = "Marketplace",
            Icon = "marketplace",
            Page = typeof(MarketplaceViewModel),
            Category = "Packages"
        });

        Menu.Add(new()
        {
            Title = "Game Mode",
            Icon = "game",
            Page = typeof(GameModeViewModel),
            Category = "Tools"
        });

        Menu.Add(new()
        {
            Title = "Identity",
            Icon = "identity",
            Page = typeof(SoftwareIdentityViewModel),
            Category = "System"
        });

        BuildMenuGrouped();
        RebuildVisibleMenu();

        SelectedMenuItem = Menu.FirstOrDefault(m => !m.IsCategoryHeader);

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

        // Installed listesi boşken Discover'a geç.
        installed.BrowseRequested += (_, _) =>
        {
            var discover = Menu.FirstOrDefault(m => m.Page == typeof(BrowseViewModel));

            if (discover is not null)
                SelectedMenuItem = discover;
        };

        // Dashboard quick action "Search" → komut paletini aç
        dashboard.SearchRequested += () => Palette.Open();

        // Detay kartları → sağ panel.
        dashboard.DetailRequested += (content, title, subtitle) =>
            ShowDetail(content, title, subtitle);

        Navigation.Navigate(_pages[typeof(DashboardViewModel)]);

        _updateService.AvailableCountChanged += () =>
            Dispatcher.UIThread.Post(UpdateBadgeCounts);

        UpdateBadgeCounts();
    }

    // Kategori başlığı sayfaya gitmez, yalnızca gerçek sayfa öğeleri seçilir.
    partial void OnSelectedMenuItemChanged(NavigationItem? value)
    {
        if (value is null || value.IsCategoryHeader)
            return;

        // Seçili sayfanın kategorisi kapalıysa aç — seçili öğe menüde gizli kalmasın.
        if (value.ParentCategory is { } header && !header.IsExpanded)
        {
            header.IsExpanded = true;
            RebuildVisibleMenu();
        }

        if (!_pages.TryGetValue(value.Page, out var page))
            return;

        if (!Navigation.Navigate(page))
            return;

        OnPropertyChanged(nameof(StatusText));

        // Sayfa ilk ziyarette yüklenir; hata zincire aktarılır (ARTICLE VII).
        _ = InitializePageSafelyAsync(page);
    }

    // Grup tıklanırsa yalnızca o grup açılır/kapanır. Diğer gruplara
    // dokunulmaz — gezinme/başlık tıklaması kullanıcının açık bıraktığı
    // bölümleri asla kapatmaz (çoklu açık grup desteklenir).
    [RelayCommand]
    private void ToggleGroup(NavigationItem? header)
    {
        if (header is null || !header.IsCategoryHeader)
            return;

        // Aktif sayfanın bölümü kapatılamaz — seçili sayfa her zaman görünür kalır.
        if (SelectedMenuItem is { IsCategoryHeader: false } selected &&
            selected.ParentCategory == header)
        {
            return;
        }

        header.IsExpanded = !header.IsExpanded;
        RebuildVisibleMenu();
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

    private void UpdateBadgeCounts()
    {
        var updatesItem = Menu.FirstOrDefault(m => m.Page == typeof(UpdatesViewModel));
        if (updatesItem is not null)
            updatesItem.BadgeCount = _updateService.AvailableCount;
    }

    private void BuildMenuGrouped()
    {
        var categoryDefs = new (string Title, string Icon)[]
        {
            ("Home", "home"),
            ("Packages", "package"),
            ("Workflows", "automation"),
            ("System", "security"),
            ("Tools", "studio"),
            ("Settings", "settings")
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
