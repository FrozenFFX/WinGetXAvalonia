// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinGetXAvalonia.Core.Domain.Dashboard;
using WinGetXAvalonia.Core.Domain.Package;
using WinGetXAvalonia.Core.Events;
using WinGetXAvalonia.Helpers;
using WinGetXAvalonia.Interfaces;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Core;
using WinGetXAvalonia.Services.History;
using WinGetXAvalonia.Services.Performance;
using WinGetXAvalonia.Services.Theme;
using WinGetXAvalonia.ViewModels.Settings;

namespace WinGetXAvalonia.ViewModels;

public partial class DashboardViewModel : ViewModelBase, IDisposable
{
    private readonly WingetService _winget;
    private readonly ISystemMonitorService _monitor;
    private readonly ITimeService _time;
    private readonly FavoritesService _favorites;
    private readonly SystemInfoService _system;
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

    private IDisposable? _subscription;

    private SystemCardModel? _cpuCard;
    private SystemCardModel? _ramCard;
    private SystemCardModel? _diskCard;
    private SystemCardModel? _netCard;
    private SystemCardModel? _timeCard;
    private HeroInfoItem? _wingetChip;
    private bool _initialized;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusText))]
    [NotifyPropertyChangedFor(nameof(TotalPackages))]
    private int installed;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusText))]
    [NotifyPropertyChangedFor(nameof(TotalPackages))]
    [NotifyPropertyChangedFor(nameof(HasUpdates))]
    private int updates;

    [ObservableProperty]
    private int favorites;

    [ObservableProperty]
    private int sources;

    [ObservableProperty]
    private int history;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusText))]
    private bool isLoading;

    [ObservableProperty]
    private string searchText = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsHealthy))]
    [NotifyPropertyChangedFor(nameof(HealthScore))]
    [NotifyPropertyChangedFor(nameof(HealthStatusText))]
    private PackageHealthSummary healthSummary = PackageHealthSummary.Empty;

    [ObservableProperty]
    private string wingetVersion = "N/A";

    [ObservableProperty]
    private bool systemPanelVisible = true;

    public event Action? SearchRequested;

    public event Action<object, string, string?>? DetailRequested;

    public string GreetingText =>
        $"👋 {Time.Greeting}, {Environment.UserName}";

    public string StatusText =>
        $"{Installed:N0} Installed • {Updates:N0} Updates";

    public string OsDisplayName =>
        SystemInfoService.GetOsLabel();

    public string OsSubtitle =>
        $"{_system.GetMachineName()}";

    public int TotalPackages =>
        Installed + Updates;

    public double Usage =>
        _system.GetRamUsage();

    public bool HasUpdates =>
        Updates > 0;

    public bool IsHealthy =>
        HealthSummary.Failed == 0;

    public int HealthScore
    {
        get
        {
            if (HealthSummary.Total == 0)
                return 0;

            int unhealthy = HealthSummary.Deprecated
                + HealthSummary.UnknownSource
                + HealthSummary.Failed;

            return (int)Math.Round(
                100.0 * (HealthSummary.Total - unhealthy) / HealthSummary.Total);
        }
    }

    public string HealthStatusText => HealthScore switch
    {
        >= 70 => "Everything is working normally.",
        >= 40 => "Some packages need attention.",
        _ => "Your package setup needs attention."
    };

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
        WingetService winget,
        ISystemMonitorService monitor,
        ITimeService time,
        FavoritesService favorites,
        SystemInfoService system,
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
        HistoryViewModel historyPage)
    {
        _winget = winget;
        _monitor = monitor;
        _time = time;
        _favorites = favorites;
        _system = system;
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

        Time = time;
        Queue = queue;
        Notifications = notifications;
        _time.Start();

        _time.PropertyChanged += OnTimeChanged;

        _monitor.PropertyChanged += OnMonitorChanged;
        _monitor.Start();

        _clock.Tick += OnClockTick;

        _subscription =
            _eventBus.Subscribe<PackageChangedEvent>(e =>
                Dispatcher.UIThread.Post(() => _ = LoadCountsAsync()));

        BuildSystemCards();
        BuildHeroInfo();
        BuildMockActivity();

        Activity.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasActivity));
        FavoriteSources.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasFavoriteSources));
        Notifications.Items.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasNotifications));
    }

    public override async Task InitializeAsync()
    {
        // ana ekran her seferinde baştan yüklenmesin, bir kez yeter
        if (_initialized)
            return;

        _initialized = true;
        IsLoading = true;

        await Task.Delay(200);
        await LoadCountsAsync();
        await LoadWingetVersionAsync();

        IsLoading = false;

#if DEBUG
        System.IO.File.AppendAllText(
            @"C:\Users\depgo\AppData\Local\Temp\opencode\statcard.log",
            $"[{DateTime.Now:HH:mm:ss.fff}] INITIALIZE done: Installed={Installed} Updates={Updates} Sources={Sources} History={History} Favorites={Favorites} IsLoading={IsLoading}{Environment.NewLine}");
#endif
    }

    private async Task LoadWingetVersionAsync()
    {
        try
        {
            var version = await _winget.Version();
            WingetVersion = version.Success ? version.Output.Trim() : "N/A";

            if (_wingetChip is not null)
                _wingetChip.Value = WingetVersion;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to load winget version");
            WingetVersion = "N/A";
            if (_wingetChip is not null)
                _wingetChip.Value = "N/A";
        }
    }

    private async Task LoadCountsAsync()
    {
        try
        {
            var installedTask = _winget.GetInstalledApps();
            var updatesTask = _winget.GetUpdates();
            var sourcesTask = _winget.GetSources();
            var historyTask = _history.GetAllAsync();

            await Task.WhenAll(installedTask, updatesTask, sourcesTask, historyTask);

            var installedLines = ToLines(installedTask.Result);
            var updateLines = ToLines(updatesTask.Result);
            var sourceLines = ToLines(sourcesTask.Result);

            var stats = DashboardStatisticsCalculator.Calculate(
                installedLines: installedLines,
                updateLines: updateLines,
                sourceLines: sourceLines,
                favoriteCount: _favorites.Favorites.Count);

            HealthSummary = PackageHealthSummaryCalculator.Calculate(
                WingetParser.ParseInstalled(installedLines),
                WingetParser.ParseUpdates(updateLines),
                historyTask.Result);

            BuildActivity(historyTask.Result);

            Installed = stats.InstalledCount;
            Updates = stats.UpdateCount;
            Sources = stats.SourceCount;
            History = historyTask.Result.Count;
            Favorites = stats.FavoriteCount;

            BuildFavoriteSources();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to load dashboard counts");

            Installed = 0;
            Updates = 0;
            Sources = 0;
            Favorites = _favorites.Favorites.Count;
            HealthSummary = PackageHealthSummary.Empty;
        }
    }

    private static List<string> ToLines(WinGetXAvalonia.Models.WingetResult result)
        => result.Success
            ? result.Output
                .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList()
            : new List<string>();

    private void BuildHeroInfo()
    {
        HeroInfo.Add(new()
        {
            Icon = "🖥️",
            Label = "OS",
            Value = GetOsLabel()
        });

        HeroInfo.Add(new()
        {
            Icon = "⚙️",
            Label = ".NET",
            Value = $"{Environment.Version.Major}.{Environment.Version.Minor}"
        });

        _wingetChip = new HeroInfoItem
        {
            Icon = "🧰",
            Label = "Winget",
            Value = "…"
        };

        HeroInfo.Add(_wingetChip);

        HeroInfo.Add(new()
        {
            Icon = "🎨",
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
            Value = _system.GetWindowsVersion(),
            Icon = "🪟"
        });

        SystemCards.Add(new()
        {
            Title = "User",
            Value = Environment.UserName,
            Icon = "👤"
        });

        _cpuCard = new SystemCardModel { Title = "CPU", Value = "...", Icon = "🧠" };
        _ramCard = new SystemCardModel { Title = "RAM", Value = "...", Icon = "💾" };
        _diskCard = new SystemCardModel { Title = "Disk", Value = "...", Icon = "💿" };
        _netCard = new SystemCardModel { Title = "Network", Value = "...", Icon = "🌐" };

        SystemCards.Add(_cpuCard);
        SystemCards.Add(_ramCard);
        SystemCards.Add(_diskCard);
        SystemCards.Add(_netCard);

        SystemCards.Add(new()
        {
            Title = "Date",
            Value = _clock.CurrentDate,
            Icon = "📅"
        });

        _timeCard = new SystemCardModel
        {
            Title = "Time",
            Value = _clock.CurrentTime,
            Icon = "🕒"
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

    private void BuildActivity(IReadOnlyList<HistoryEntry> history)
    {
        Activity.Clear();

        foreach (var entry in history
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
    private void OpenSettings()
        => _navigation.Navigate(_settingsPage);

    [RelayCommand]
    private Task Refresh()
        => InitializeAsync();

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

        foreach (var item in _system.GetOsDetails())
            vm.Items.Add(item);

        DetailRequested?.Invoke(vm, "System", _system.GetMachineName());
    }

    [RelayCommand]
    private void ShowNotificationsDetails()
    {
        var vm = new DetailListViewModel();

        foreach (var item in Notifications.Items)
        {
            vm.Items.Add(new HeroInfoItem
            {
                Icon = item.IsRead ? "🔕" : "🔔",
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
            => vm.Items.Add(new HeroInfoItem { Icon = "🩺", Label = label, Value = value });

        Add("Score", $"{HealthScore}%");
        Add("Total", HealthSummary.Total.ToString());
        Add("Healthy", HealthSummary.Healthy.ToString());
        Add("Outdated", HealthSummary.Outdated.ToString());
        Add("Deprecated", HealthSummary.Deprecated.ToString());
        Add("Unknown Source", HealthSummary.UnknownSource.ToString());
        Add("Failed", HealthSummary.Failed.ToString());
        Add("Status", HealthStatusText);

        DetailRequested?.Invoke(vm, "System Health", $"{HealthScore}% score");
    }

    [RelayCommand]
    private void Search()
        => SearchRequested?.Invoke();

    [RelayCommand]
    private void OpenWorkspace()
        => _navigation.Navigate(_workspacesPage);

    [RelayCommand]
    private async Task UpdateAll()
    {
        await _winget.UpgradeAll();
        await LoadCountsAsync();
    }

    [RelayCommand]
    private async Task RefreshSources()
    {
        await _winget.RefreshSources();
        await LoadCountsAsync();
    }

    public void Dispose()
    {
        _time.PropertyChanged -= OnTimeChanged;
        _monitor.PropertyChanged -= OnMonitorChanged;
        _clock.Tick -= OnClockTick;
        _subscription?.Dispose();
    }
}
