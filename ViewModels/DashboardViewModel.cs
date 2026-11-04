// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinGetXAvalonia.Core.State;
using WinGetXAvalonia.Core.Domain.Package;
using WinGetXAvalonia.Core.Events;
using WinGetXAvalonia.Core.Providers;
using WinGetXAvalonia.Interfaces;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Core;
using WinGetXAvalonia.Services.Dialogs;
using WinGetXAvalonia.Services.History;
using WinGetXAvalonia.Services.Interfaces;
using WinGetXAvalonia.Services.Notifications;
using WinGetXAvalonia.Services.Operations;
using WinGetXAvalonia.Services.Performance;
using WinGetXAvalonia.Services.Sources;
using WinGetXAvalonia.Services.Theme;
using WinGetXAvalonia.Services.Updates;
using WinGetXAvalonia.ViewModels.Settings;

namespace WinGetXAvalonia.ViewModels;

public partial class DashboardViewModel : ViewModelBase, IDisposable
{
    private readonly IPackageProvider _provider;
    private readonly ISourceManagerService _sources;
    private readonly WingetService _winget;
    private readonly IUpdateService _updates;
    private readonly IDialogService _dialogs;
    private readonly ISettingsService _settings;
    private readonly NotificationService _notificationService;
    private readonly ISystemMonitorService _monitor;
    private readonly ITimeService _time;
    private readonly FavoritesService _favorites;
    private SystemInformation _systemInfo = SystemInformation.Empty;

    public SystemInformation SystemInfo
    {
        get => _systemInfo;
        set
        {
            if (_systemInfo != value)
            {
                _systemInfo = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(OsDisplayName));
                OnPropertyChanged(nameof(OsSubtitle));
                OnPropertyChanged(nameof(Usage));
            }
        }
    }
    private readonly IThemeService _theme;
    private readonly ClockService _clock;
    private readonly NavigationService _navigation;
    private readonly LoggerService _logger;
    private readonly IEventBus _eventBus;
    private readonly IHistoryService _history;
    private readonly InstalledViewModel _installedPage;
    private readonly SettingsViewModel _settingsPage;
    private readonly WorkspacesViewModel _workspacesPage;
    private readonly BrowseViewModel _browsePage;
    private readonly HistoryViewModel _historyPage;
    private readonly UpdatesViewModel _updatesPage;

    private IDisposable? _subscription;
    private CancellationTokenSource? _loadCts;
    private CancellationTokenSource? _updateAllCts;

    private SystemCardModel? _cpuCard;
    private SystemCardModel? _ramCard;
    private SystemCardModel? _diskCard;
    private SystemCardModel? _netCard;
    private SystemCardModel? _dateCard;
    private SystemCardModel? _timeCard;
    private HeroInfoItem? _wingetChip;

    private bool _initialized;
    private bool _wingetStatsLoaded;
    private bool _historyStatsLoaded;

    private IReadOnlyList<InstalledPackage> _installedPackages = Array.Empty<InstalledPackage>();
    private IReadOnlyList<UpdatePackage> _updatePackages = Array.Empty<UpdatePackage>();
    private IReadOnlyList<HistoryEntry> _historyEntries = Array.Empty<HistoryEntry>();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusText))]
    [NotifyPropertyChangedFor(nameof(TotalPackages))]
    [NotifyPropertyChangedFor(nameof(InstalledDisplay))]
    private int installed;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusText))]
    [NotifyPropertyChangedFor(nameof(TotalPackages))]
    [NotifyPropertyChangedFor(nameof(HasUpdates))]
    [NotifyPropertyChangedFor(nameof(CanUpdateAll))]
    [NotifyCanExecuteChangedFor(nameof(UpdateAllCommand))]
    [NotifyPropertyChangedFor(nameof(UpdatesDisplay))]
    private int updates;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FavoritesDisplay))]
    private int favorites;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SourcesDisplay))]
    private int sources;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HistoryDisplay))]
    private int history;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusText))]
    private bool isLoading;

    [ObservableProperty]
    private bool isRefreshing;

    [ObservableProperty]
    private string searchText = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsHealthy))]
    [NotifyPropertyChangedFor(nameof(HealthScore))]
    [NotifyPropertyChangedFor(nameof(HealthScoreDisplay))]
    [NotifyPropertyChangedFor(nameof(HealthProgressValue))]
    [NotifyPropertyChangedFor(nameof(HealthStatusText))]
    [NotifyPropertyChangedFor(nameof(IsHealthLoaded))]
    private PackageHealthSummary healthSummary = PackageHealthSummary.Empty;

    [ObservableProperty]
    private string wingetVersion = "N/A";

    [ObservableProperty]
    private bool systemPanelVisible = true;

    [ObservableProperty]
    private bool hasError;

    [ObservableProperty]
    private string? errorMessage;

    [ObservableProperty]
    private string? wingetError;

    [ObservableProperty]
    private string? historyError;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanUpdateAll))]
    [NotifyCanExecuteChangedFor(nameof(UpdateAllCommand))]
    private bool isUpdateAllRunning;

    [ObservableProperty]
    private string updateAllStatus = "";

    [ObservableProperty]
    private string updateAllCurrentPackage = "";

    [ObservableProperty]
    private string? updateAllResultText;

    public event Action? SearchRequested;

    public event Action<object, string, string?>? DetailRequested;

    public string GreetingText =>
        $"{Time.Greeting}, {Environment.UserName}";

    public string StatusText =>
        !_wingetStatsLoaded
            ? "Loading package statistics…"
            : $"{Installed:N0} Installed • {Updates:N0} Updates";

    public string OsDisplayName =>
        SystemInfoService.GetOsLabel();

    public string OsSubtitle =>
        $"{SystemInfo.MachineName}";

    public int TotalPackages =>
        Installed + Updates;

    public double? Usage =>
        SystemInfo?.RamUsage;

    public bool HasUpdates =>
        Updates > 0;

    public bool CanUpdateAll =>
        !IsUpdateAllRunning && HasUpdates && _wingetStatsLoaded;

    public bool IsHealthy =>
        HealthSummary.Failed == 0;

    public bool IsHealthLoaded =>
        _wingetStatsLoaded && _historyStatsLoaded;

    public string InstalledDisplay =>
        _wingetStatsLoaded ? $"{Installed:N0}" : "…";

    public string UpdatesDisplay =>
        _wingetStatsLoaded ? $"{Updates:N0}" : "…";

    public string HistoryDisplay =>
        _historyStatsLoaded ? $"{History:N0}" : "…";

    public string SourcesDisplay =>
        _wingetStatsLoaded ? $"{Sources:N0}" : "…";

    public string FavoritesDisplay =>
        _wingetStatsLoaded ? $"{Favorites:N0}" : "…";

    public int HealthScore
    {
        get
        {
            if (!IsHealthLoaded || HealthSummary.Total == 0)
                return 0;

            int healthyCount = HealthSummary.Healthy;
            int total = HealthSummary.Total;

            return (int)Math.Round(
                100.0 * healthyCount / total);
        }
    }

    public double HealthProgressValue =>
        IsHealthLoaded && HealthSummary.Total > 0 ? HealthScore : 0;

    public string HealthScoreDisplay =>
        !IsHealthLoaded ? "…"
        : HealthSummary.Total == 0 ? "N/A"
        : $"{HealthScore}%";

    public string HealthStatusText
    {
        get
        {
            if (!IsHealthLoaded)
                return "Calculating health score…";

            if (HealthSummary.Total == 0)
                return "No installed packages to evaluate.";

            return HealthScore switch
            {
                >= 70 => "Everything is working normally.",
                >= 40 => "Some packages need attention.",
                _ => "Your package setup needs attention."
            };
        }
    }

    // Sağlık puanının nereden geldiğini açıklar; yalnızca gerçekten
    // var olan faktörler gösterilir (ARTICLE: uydurma veri yok).
    public string HealthFactorsText
    {
        get
        {
            if (!IsHealthLoaded || HealthSummary.Total == 0)
                return "";

            var factors = new List<string>
            {
                $"{HealthSummary.Healthy} healthy"
            };

            if (HealthSummary.Outdated > 0)
                factors.Add($"{HealthSummary.Outdated} outdated");

            if (HealthSummary.Deprecated > 0)
                factors.Add($"{HealthSummary.Deprecated} deprecated");

            if (HealthSummary.UnknownSource > 0)
                factors.Add($"{HealthSummary.UnknownSource} unknown source");

            if (HealthSummary.Failed > 0)
                factors.Add($"{HealthSummary.Failed} failed operations");

            return $"Based on {HealthSummary.Total} installed packages: "
                + string.Join(", ", factors) + ".";
        }
    }

    public ObservableCollection<SystemCardModel> SystemCards { get; } = new();

    public ObservableCollection<FavoriteSource> FavoriteSources { get; } = new();

    public ObservableCollection<ActivityItem> Activity { get; } = new();

    public ObservableCollection<HeroInfoItem> HeroInfo { get; } = new();

    public bool HasActivity => Activity.Count > 0;
    public bool HasNoActivity => !HasActivity;
    public bool HasFavoriteSources => FavoriteSources.Count > 0;
    public bool HasNoFavoriteSources => !HasFavoriteSources;
    public bool HasNotifications => !Notifications.IsEmpty;
    public bool HasNoNotifications => !HasNotifications;

    public ITimeService Time { get; }

    public QueuePanelViewModel Queue { get; }

    public NotificationCenterViewModel Notifications { get; }

    public DashboardViewModel(
        IPackageProvider provider,
        ISourceManagerService sources,
        WingetService winget,
        IUpdateService updates,
        ISystemMonitorService monitor,
        ITimeService time,
        FavoritesService favorites,
        SystemInformation system,
        IThemeService theme,
        ClockService clock,
        NavigationService navigation,
        LoggerService logger,
        IEventBus eventBus,
        IHistoryService history,
        QueuePanelViewModel queue,
        NotificationCenterViewModel notifications,
        InstalledViewModel installedPage,
        SettingsViewModel settingsPage,
        WorkspacesViewModel workspacesPage,
        BrowseViewModel browsePage,
        HistoryViewModel historyPage,
        UpdatesViewModel updatesPage,
        IDialogService dialogs,
        ISettingsService settings,
        NotificationService notificationService)
    {
        _provider = provider;
        _sources = sources;
        _winget = winget;
        _updates = updates;
        _dialogs = dialogs;
        _settings = settings;
        _notificationService = notificationService;
        _monitor = monitor;
        _time = time;
        _favorites = favorites;
        _systemInfo = system;
        _theme = theme;
        _clock = clock;
        _navigation = navigation;
        _logger = logger;
        _eventBus = eventBus;
        _history = history;
        _installedPage = installedPage;
        _settingsPage = settingsPage;
        _workspacesPage = workspacesPage;
        _browsePage = browsePage;
        _historyPage = historyPage;
        _updatesPage = updatesPage;

        Time = time;
        Queue = queue;
        Notifications = notifications;

        _time.PropertyChanged += OnTimeChanged;
        _monitor.PropertyChanged += OnMonitorChanged;
        _clock.Tick += OnClockTick;
        _clock.DateChanged += OnDateChanged;

        _subscription =
            _eventBus.Subscribe<PackageChangedEvent>(pkg =>
                Dispatcher.UIThread.Post(() => _ = Refresh()));

        BuildSystemCards();
        BuildHeroInfo();
        BuildMockActivity();

        Activity.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasActivity));
        FavoriteSources.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasFavoriteSources));
        Notifications.Items.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasNotifications));

        _time.Start();
        _monitor.Start();
    }

    public override async Task InitializeAsync()
    {
        if (_initialized)
            return;

        await LoadDashboardAsync(isInitial: true);
    }

    [RelayCommand(CanExecute = nameof(CanRefresh))]
    private Task Refresh()
        => LoadDashboardAsync(isInitial: false);

    private bool CanRefresh() => !IsLoading && !IsRefreshing;

    private async Task LoadDashboardAsync(bool isInitial)
    {
        if (isInitial)
        {
            if (_initialized)
                return;
        }
        else if (IsLoading || IsRefreshing)
        {
            return;
        }

        _loadCts?.Cancel();
        _loadCts?.Dispose();
        _loadCts = new CancellationTokenSource();
        var cancellationToken = _loadCts.Token;

        if (isInitial)
            IsLoading = true;
        else
            IsRefreshing = true;

        RefreshCommand.NotifyCanExecuteChanged();

        try
        {
            _logger.Log("Dashboard initialization started");
            HasError = false;
            ErrorMessage = null;

            _logger.Log("Loading system information");
            _logger.Log("Loading Winget information");
            _logger.Log("Loading history");
            _logger.Log("Loading notifications");

            var loadTask = Task.WhenAll(
                LoadWingetInformationAsync(cancellationToken),
                LoadHistoryInformationAsync(cancellationToken),
                LoadWingetVersionAsync(cancellationToken),
                LoadNotificationsAsync(cancellationToken));

            var timeout = Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            await Task.WhenAny(loadTask, timeout);

            _initialized = true;

            _logger.Log("Dashboard initialization completed");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.Log("Dashboard load cancelled");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Dashboard initialization failed");
            HasError = true;
            ErrorMessage = ex.Message;
        }
        finally
        {
            if (isInitial)
                IsLoading = false;
            else
                IsRefreshing = false;

            RefreshCommand.NotifyCanExecuteChanged();
            NotifyStatDisplaysChanged();
        }
    }

    private async Task LoadWingetInformationAsync(CancellationToken cancellationToken)
    {
        _logger.Log("Loading Winget information");

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var installed = await _provider.GetInstalledAsync().ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();

            var updates = await _updates.GetUpdatesAsync(cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();

            IReadOnlyList<WingetSource> sourceList;
            try
            {
                sourceList = await _sources.GetSourcesAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Loading Winget sources failed");
                sourceList = Array.Empty<WingetSource>();
                WingetError = "Sources unavailable";
            }

            cancellationToken.ThrowIfCancellationRequested();

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                _installedPackages = installed;
                _updatePackages = updates;

                Installed = installed.Count;
                Updates = updates.Count;
                Sources = sourceList.Count;
                Favorites = _favorites.Favorites.Count;

                _wingetStatsLoaded = true;
                WingetError = installed.Count == 0 && updates.Count == 0
                    ? "No packages found or Winget unavailable"
                    : null;

                BuildFavoriteSources();
                UpdateHealthSummary();
                NotifyStatDisplaysChanged();

                _logger.Log(
                    $"Winget information loaded: {Installed} installed, {Updates} updates, {Sources} sources");
            });
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Loading Winget information failed");

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                WingetError = "Winget unavailable";

                if (!_wingetStatsLoaded)
                {
                    Installed = 0;
                    Updates = 0;
                    Sources = 0;
                    Favorites = _favorites.Favorites.Count;
                    HealthSummary = PackageHealthSummary.Empty;
                }

                NotifyStatDisplaysChanged();
            });
        }
    }

    private async Task LoadHistoryInformationAsync(CancellationToken cancellationToken)
    {
        _logger.Log("Loading history");

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var entries = await _history.GetAllAsync().ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                _historyEntries = entries;
                History = entries.Count;
                HistoryError = null;
                _historyStatsLoaded = true;

                BuildActivity(entries);
                UpdateHealthSummary();
                NotifyStatDisplaysChanged();

                _logger.Log($"History loaded: {History} entries");
            });
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Loading history failed");

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                HistoryError = "History unavailable";

                if (!_historyStatsLoaded)
                {
                    History = 0;
                    Activity.Clear();
                }

                NotifyStatDisplaysChanged();
            });
        }
    }

    private async Task LoadWingetVersionAsync(CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var version = await _winget.Version().ConfigureAwait(false);

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                WingetVersion = version.Success ? version.Output.Trim() : "N/A";

                if (_wingetChip is not null)
                    _wingetChip.Value = WingetVersion;
            });
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to load winget version");

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                WingetVersion = "N/A";

                if (_wingetChip is not null)
                    _wingetChip.Value = "N/A";
            });
        }
    }

    private async Task LoadNotificationsAsync(CancellationToken cancellationToken)
    {
        _logger.Log("Loading notifications");

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Notifications.Refresh();
                OnPropertyChanged(nameof(HasNotifications));
                OnPropertyChanged(nameof(HasNoNotifications));
            });
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Loading notifications failed");
        }
    }

    private void UpdateHealthSummary()
    {
        if (!_wingetStatsLoaded || !_historyStatsLoaded)
            return;

        HealthSummary = PackageHealthSummaryCalculator.Calculate(
            _installedPackages,
            _updatePackages,
            _historyEntries);
    }

    private void NotifyStatDisplaysChanged()
    {
        OnPropertyChanged(nameof(InstalledDisplay));
        OnPropertyChanged(nameof(UpdatesDisplay));
        OnPropertyChanged(nameof(HistoryDisplay));
        OnPropertyChanged(nameof(SourcesDisplay));
        OnPropertyChanged(nameof(FavoritesDisplay));
        OnPropertyChanged(nameof(IsHealthLoaded));
        OnPropertyChanged(nameof(HealthScoreDisplay));
        OnPropertyChanged(nameof(HealthProgressValue));
        OnPropertyChanged(nameof(HealthStatusText));
        OnPropertyChanged(nameof(HealthFactorsText));
        OnPropertyChanged(nameof(CanUpdateAll));
        OnPropertyChanged(nameof(StatusText));
        UpdateAllCommand.NotifyCanExecuteChanged();
    }

    private void BuildHeroInfo()
    {
        HeroInfo.Add(new()
        {
            Icon = "windows",
            Label = "OS",
            Value = GetOsLabel()
        });

        HeroInfo.Add(new()
        {
            Icon = "developer",
            Label = ".NET",
            Value = $"{Environment.Version.Major}.{Environment.Version.Minor}"
        });

        _wingetChip = new HeroInfoItem
        {
            Icon = "package",
            Label = "Winget",
            Value = "…"
        };

        HeroInfo.Add(_wingetChip);

        HeroInfo.Add(new()
        {
            Icon = "settings",
            Label = "Theme",
            Value = _theme.CurrentTheme.ToString()
        });
    }

    private static string GetOsLabel()
        => Environment.OSVersion.Version.Build >= 22000 ? "Windows 11" : "Windows 10";

    private void BuildSystemCards()
    {
        SystemCards.Add(new()
        {
            Title = "OS",
            Value = SystemInfo?.WindowsVersion ?? "N/A",
            Icon = "windows"
        });

        SystemCards.Add(new()
        {
            Title = "User",
            Value = Environment.UserName,
            Icon = "user"
        });

        _cpuCard = new SystemCardModel { Title = "CPU", Value = "...", Icon = "cpu" };
        _ramCard = new SystemCardModel { Title = "RAM", Value = "...", Icon = "ram" };
        _diskCard = new SystemCardModel { Title = "Disk", Value = "...", Icon = "disk" };
        _netCard = new SystemCardModel { Title = "Network", Value = "...", Icon = "network" };

        SystemCards.Add(_cpuCard);
        SystemCards.Add(_ramCard);
        SystemCards.Add(_diskCard);
        SystemCards.Add(_netCard);

        _dateCard = new SystemCardModel
        {
            Title = "Date",
            Value = _clock.CurrentDate,
            Icon = "calendar"
        };

        SystemCards.Add(_dateCard);

        _timeCard = new SystemCardModel
        {
            Title = "Time",
            Value = _clock.CurrentTime,
            Icon = "clock"
        };

        SystemCards.Add(_timeCard);
    }

    private void BuildMockActivity()
    {
        Activity.Clear();
    }

    private void BuildFavoriteSources()
    {
        FavoriteSources.Clear();

        foreach (var group in _favorites.Favorites
                     .Where(p => !string.IsNullOrWhiteSpace(p.Source))
                     .GroupBy(p => p.Source)
                     .OrderByDescending(g => g.Count())
                     .Take(6))
        {
            FavoriteSources.Add(new FavoriteSource
            {
                Name = group.Key,
                Count = group.Count()
            });
        }
    }

    private void BuildActivity(IReadOnlyList<HistoryEntry> historyEntries)
    {
        Activity.Clear();

        foreach (var entry in historyEntries
                     .OrderByDescending(e => e.Timestamp)
                     .Take(8))
        {
            Activity.Add(new ActivityItem
            {
                Timestamp = entry.Timestamp,
                Title = string.IsNullOrWhiteSpace(entry.PackageName)
                    ? (string.IsNullOrWhiteSpace(entry.PackageId) ? "Package operation" : entry.PackageId)
                    : entry.PackageName,
                Type = ToActivityType(entry.Operation, entry.Success)
            });
        }
    }

    private static ActivityType ToActivityType(string operation, bool success)
    {
        if (!success)
            return ActivityType.Error;

        return operation switch
        {
            "Install" => ActivityType.Install,
            "Update" => ActivityType.Upgrade,
            "Remove" => ActivityType.Uninstall,
            _ => ActivityType.Info
        };
    }

    private void OnTimeChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ITimeService.Greeting))
            OnPropertyChanged(nameof(GreetingText));
    }

    private void OnMonitorChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(ISystemMonitorService.CpuUsage):
                if (_cpuCard is not null)
                    _cpuCard.Value = $"{_monitor.CpuUsage:0.0} %";
                break;

            case nameof(ISystemMonitorService.RamUsage):
                if (_ramCard is not null)
                    _ramCard.Value = $"{_monitor.RamUsage:0.0} %";
                break;

            case nameof(ISystemMonitorService.DiskUsage):
                if (_diskCard is not null)
                    _diskCard.Value = $"{_monitor.DiskUsage:0.0} %";
                break;

            case nameof(ISystemMonitorService.NetworkUsage):
                if (_netCard is not null)
                    _netCard.Value = FormatNetwork(_monitor.NetworkUsage);
                break;
        }
    }

    private void OnClockTick(string time)
    {
        if (_timeCard is not null)
            _timeCard.Value = time;
    }

    private void OnDateChanged(string date)
    {
        if (_dateCard is not null)
            _dateCard.Value = date;
    }

    private static string FormatNetwork(double bytesPerSecond)
    {
        if (bytesPerSecond >= 1024 * 1024)
            return $"{bytesPerSecond / (1024 * 1024):0.0} MB/s";

        if (bytesPerSecond >= 1024)
            return $"{bytesPerSecond / 1024:0.0} KB/s";

        return $"{bytesPerSecond:0.0} B/s";
    }

    [RelayCommand]
    private void OpenPackages()
    {
        _navigation.Navigate(_installedPage);
        _ = _installedPage.InitializeAsync();
    }

    [RelayCommand]
    private void Browse()
    {
        _navigation.Navigate(_browsePage);
        _ = _browsePage.InitializeAsync();
    }

    [RelayCommand]
    private void OpenHistory()
    {
        _navigation.Navigate(_historyPage);
        _ = _historyPage.InitializeAsync();
    }

    [RelayCommand]
    private void OpenUpdates()
    {
        _navigation.Navigate(_updatesPage);
        _ = _updatesPage.InitializeAsync();
    }

    [RelayCommand]
    private void OpenSettings()
        => _navigation.Navigate(_settingsPage);

    [RelayCommand]
    private void CloseSystemPanel()
    {
        SystemPanelVisible = false;
        _logger.Log("System panel closed");
    }

    [RelayCommand]
    private void ShowSystemDetails()
    {
        var vm = new DetailListViewModel();

        void Add(string label, string value)
            => vm.Items.Add(new HeroInfoItem { Icon = "diagnostics", Label = label, Value = value });

        Add("OS", SystemInfo?.WindowsVersion ?? "N/A");
        Add("Architecture", SystemInfo?.Architecture ?? "N/A");
        Add("MachineName", SystemInfo?.MachineName ?? "N/A");
        Add("User", SystemInfo?.UserName ?? "N/A");
        Add("CPU", SystemInfo?.CpuName ?? "N/A");
        Add("Threads", SystemInfo?.CpuThreads?.ToString() ?? "N/A");
        Add("RAM", $"{SystemInfo?.RamUsed:0.0} / {SystemInfo?.RamTotal:0.0} GB");
        Add("GPU", SystemInfo?.GpuName ?? "N/A");
        Add("Network", SystemInfo?.Network ?? "N/A");
        Add(".NET", SystemInfo?.DotNetVersion ?? "N/A");
        Add("Winget", SystemInfo?.WingetVersion ?? "N/A");

        DetailRequested?.Invoke(vm, "System", $"{SystemInfo?.OperatingSystem} {SystemInfo?.WindowsVersion ?? ""}".Trim());
    }

    [RelayCommand]
    private void ShowNotificationsDetails()
    {
        var vm = new DetailListViewModel();

        foreach (var item in Notifications.Items)
        {
            vm.Items.Add(new HeroInfoItem
            {
                Icon = item.IsRead ? "notification" : "notification",
                Label = item.Title,
                Value = $"{item.Timestamp:HH:mm} • {item.Message}"
            });
        }

        DetailRequested?.Invoke(
            vm,
            "Notifications",
            Notifications.Items.Count == 0
                ? "No notifications"
                : $"{Notifications.UnreadCount} unread");
    }

    [RelayCommand]
    private void ShowHealthDetails()
    {
        var vm = new DetailListViewModel();

        void Add(string label, string value)
            => vm.Items.Add(new HeroInfoItem { Icon = "diagnostics", Label = label, Value = value });

        Add("Score", HealthScoreDisplay);
        Add("Total", HealthSummary.Total.ToString());
        Add("Healthy", HealthSummary.Healthy.ToString());
        Add("Outdated", HealthSummary.Outdated.ToString());
        Add("Deprecated", HealthSummary.Deprecated.ToString());
        Add("Status", HealthStatusText);

        DetailRequested?.Invoke(vm, "System Health", $"{HealthScoreDisplay} score");
    }

    [RelayCommand]
    private void Search()
        => SearchRequested?.Invoke();

    [RelayCommand]
    private void OpenWorkspace()
        => _navigation.Navigate(_workspacesPage);

    // Update All: otoriter güncelleme listesini (IUpdateService) kullanır,
    // kullanıcı seçiminden bağımsızdır ve Update Selected ile karışmaz.
    [RelayCommand(CanExecute = nameof(CanUpdateAll))]
    private async Task UpdateAll()
    {
        if (IsUpdateAllRunning)
            return;

        var available = await _updates.GetUpdatesAsync();

        if (available.Count == 0)
        {
            _notificationService.Notify(
                NotificationType.Information,
                "No updates available",
                "All packages are up to date.");
            return;
        }

        if (_settings.Settings.ConfirmUpdate)
        {
            var confirmed = await _dialogs.ConfirmAsync(
                "Update all packages?",
                $"{available.Count} packages have updates available.\n\n" +
                "This operation will update all currently available packages.",
                "Update All",
                "Cancel");

            if (!confirmed)
                return;
        }

        IsUpdateAllRunning = true;
        UpdateAllResultText = null;
        UpdateAllStatus = "Starting...";
        UpdateAllCurrentPackage = "";

        _updateAllCts?.Dispose();
        _updateAllCts = new CancellationTokenSource();

        var progress = new Progress<BulkUpdateProgress>(p =>
        {
            UpdateAllStatus = p.Total > 0
                ? $"Updating packages… {p.Completed} / {p.Total}"
                : "Updating packages…";

            UpdateAllCurrentPackage = p.CurrentPackage;

            if (p.Failed > 0 || p.CancelledCount > 0)
                UpdateAllStatus += $"  ({p.Succeeded} ok, {p.Failed} failed)";
        });

        try
        {
            var result = await _updates.UpdateAllAsync(progress, _updateAllCts.Token);

            _updates.Invalidate();
            await Refresh();

            UpdateAllResultText = result.Total == 0
                ? "No updates were applied."
                : result.SucceededAll
                    ? $"Update complete — {result.Succeeded} updated."
                    : $"Update complete — {result.Succeeded} successful, {result.Failed} failed, {result.CancelledCount} cancelled.";
        }
        catch (OperationCanceledException)
        {
            UpdateAllResultText = "Update cancelled.";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Update All failed");
            UpdateAllResultText = "Unable to update packages. Reason: WinGet operation failed.";
        }
        finally
        {
            IsUpdateAllRunning = false;
            UpdateAllCurrentPackage = "";

            _updateAllCts?.Dispose();
            _updateAllCts = null;

            UpdateAllCommand.NotifyCanExecuteChanged();
        }
    }

    [RelayCommand]
    private void CancelUpdateAll()
    {
        _updateAllCts?.Cancel();
        _logger.Log("Update All cancellation requested");
    }

    [RelayCommand]
    private async Task RefreshSources()
    {
        await _winget.RefreshSources();
        _updates.Invalidate();
        await Refresh();
    }

    public void Dispose()
    {
        _loadCts?.Cancel();
        _loadCts?.Dispose();

        _updateAllCts?.Cancel();
        _updateAllCts?.Dispose();

        _time.PropertyChanged -= OnTimeChanged;
        _monitor.PropertyChanged -= OnMonitorChanged;
        _clock.Tick -= OnClockTick;
        _clock.DateChanged -= OnDateChanged;
        _subscription?.Dispose();
    }
}
