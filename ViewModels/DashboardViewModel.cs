// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Collections.ObjectModel;
using System.ComponentModel;
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

public partial class DashboardViewModel : ViewModelBase
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
    private bool isLoading;

    [ObservableProperty]
    private string searchText = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsHealthy))]
    private PackageHealthSummary healthSummary = PackageHealthSummary.Empty;

    public string StatusText =>
        $"{Installed:N0} Installed • {Updates:N0} Updates";

    public int TotalPackages =>
        Installed + Updates;

    public bool HasUpdates =>
        Updates > 0;

    public bool IsHealthy =>
        HealthSummary.Failed == 0;

    public ObservableCollection<SystemCardModel> SystemCards { get; } = new();

    public ObservableCollection<QuickActionItem> QuickActions { get; } = new();

    public ObservableCollection<ActivityItem> Activity { get; } = new();

    public ObservableCollection<HeroInfoItem> HeroInfo { get; } = new();

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
        SettingsViewModel settingsPage)
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

        Time = time;
        Queue = queue;
        Notifications = notifications;
        _time.Start();

        _monitor.PropertyChanged += OnMonitorChanged;
        _monitor.Start();

        _clock.Tick += OnClockTick;

        _eventBus.Subscribe<PackageChangedEvent>(e => _ = LoadCountsAsync());

        BuildQuickActions();
        BuildSystemCards();
        BuildHeroInfo();
        BuildMockActivity();
    }

    public async Task InitializeAsync()
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
    }

    private async Task LoadWingetVersionAsync()
    {
        try
        {
            var version = (await _winget.Version()).Trim();

            if (_wingetChip is not null)
                _wingetChip.Value = version;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to load winget version");
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

            var stats = DashboardStatisticsCalculator.Calculate(
                installedLines: installedTask.Result,
                updateLines: updatesTask.Result,
                sourceLines: sourcesTask.Result,
                favoriteCount: _favorites.Favorites.Count);

            HealthSummary = PackageHealthSummaryCalculator.Calculate(
                WingetParser.ParseInstalled(installedTask.Result),
                WingetParser.ParseUpdates(updatesTask.Result),
                historyTask.Result);

            Installed = stats.InstalledCount;
            Updates = stats.UpdateCount;
            Sources = stats.SourceCount;
            Favorites = stats.FavoriteCount;
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

    private void BuildQuickActions()
    {
        QuickActions.Add(new()
        {
            Icon = "📦",
            Title = "Install Packages",
            Description = "Browse the winget catalog and install",
            Command = OpenPackagesCommand
        });

        QuickActions.Add(new()
        {
            Icon = "⬆️",
            Title = "Upgrade All",
            Description = "Update every installed package",
            Command = UpgradeAllCommand
        });

        QuickActions.Add(new()
        {
            Icon = "🔄",
            Title = "Refresh Sources",
            Description = "Reload winget package sources",
            Command = RefreshSourcesCommand
        });

        QuickActions.Add(new()
        {
            Icon = "⚙️",
            Title = "Open Settings",
            Description = "Tune WinGetX to your taste",
            Command = OpenSettingsCommand
        });
    }

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
        Activity.Add(new()
        {
            Timestamp = DateTime.Now.AddMinutes(-3),
            Title = "Visual Studio Code updated to 1.92.0",
            Type = ActivityType.Update
        });

        Activity.Add(new()
        {
            Timestamp = DateTime.Now.AddMinutes(-12),
            Title = "Google Chrome installed",
            Type = ActivityType.Install
        });

        Activity.Add(new()
        {
            Timestamp = DateTime.Now.AddHours(-1),
            Title = "Spotify uninstalled",
            Type = ActivityType.Uninstall
        });

        Activity.Add(new()
        {
            Timestamp = DateTime.Now.AddHours(-3),
            Title = "7-Zip upgraded to 24.09",
            Type = ActivityType.Upgrade
        });
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
    private void OpenSettings()
        => _navigation.Navigate(_settingsPage);

    [RelayCommand]
    private Task Refresh()
        => InitializeAsync();

    partial void OnSearchTextChanged(string value)
        => Search();

    private void Search()
    {
        // Arama çubuğu UI polish aşamasında ekleniyor.
    }

    [RelayCommand]
    private async Task UpgradeAll()
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
}
