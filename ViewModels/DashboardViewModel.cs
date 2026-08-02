using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services;

namespace WinGetXAvalonia.ViewModels;

public partial class DashboardViewModel : ViewModelBase
{
    private readonly WingetService _winget;
    private readonly SystemInfoService _system;
    private readonly ClockService _clock;

    private SystemCardModel? _timeCard;
    private bool _initialized;

    [ObservableProperty]
    private int installed;

    [ObservableProperty]
    private int updates;

    [ObservableProperty]
    private string wingetVersion = "...";

    [ObservableProperty]
    private bool isLoading;

    public ObservableCollection<DashboardCardModel> Cards { get; } = new();

    public ObservableCollection<SystemCardModel> SystemCards { get; } = new();

    public DashboardViewModel(
        WingetService winget,
        SystemInfoService system,
        ClockService clock)
    {
        _winget = winget;
        _system = system;
        _clock = clock;

        _clock.Tick += OnClockTick;

        BuildSystemCards();
        BuildPlaceholderCards();
    }

    public async Task InitializeAsync()
    {
        if (_initialized)
            return;

        _initialized = true;
        IsLoading = true;

        await Task.Delay(200);
        await Load();

        IsLoading = false;
    }

    private async Task Load()
    {
        var installedTask = _winget.GetInstalledApps();
        var updatesTask = _winget.GetUpdates();
        var versionTask = _winget.Version();

        await Task.WhenAll(
            installedTask,
            updatesTask,
            versionTask);

        Installed = installedTask.Result.Count;
        Updates = updatesTask.Result.Count;
        WingetVersion = versionTask.Result.Trim();

        Cards.Clear();

        Cards.Add(new()
        {
            Title = "Installed",
            Value = Installed.ToString(),
            Icon = "📦"
        });

        Cards.Add(new()
        {
            Title = "Updates",
            Value = Updates.ToString(),
            Icon = "⬇️"
        });

        Cards.Add(new()
        {
            Title = "Winget",
            Value = WingetVersion,
            Icon = "🛡️"
        });

        Cards.Add(new()
        {
            Title = "Status",
            Value = "OK",
            Icon = "✅"
        });
    }

    private void BuildPlaceholderCards()
    {
        Cards.Clear();

        Cards.Add(new()
        {
            Title = "Installed",
            Value = Installed.ToString(),
            Icon = "📦"
        });

        Cards.Add(new()
        {
            Title = "Updates",
            Value = Updates.ToString(),
            Icon = "⬇️"
        });

        Cards.Add(new()
        {
            Title = "Winget",
            Value = WingetVersion,
            Icon = "🛡️"
        });

        Cards.Add(new()
        {
            Title = "Status",
            Value = "...",
            Icon = "✅"
        });
    }

    private void BuildSystemCards()
    {
        SystemCards.Clear();

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

        SystemCards.Add(new()
        {
            Title = "CPU",
            Value = $"{_system.GetCpuThreads()} threads",
            Icon = "🧠"
        });

        SystemCards.Add(new()
        {
            Title = "RAM",
            Value = $"{_system.GetRamUsage()} %",
            Icon = "💾"
        });

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

    private void OnClockTick(string time)
    {
        if (_timeCard is not null)
            _timeCard.Value = time;
    }
}
