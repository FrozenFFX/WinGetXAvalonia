// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using WinGetXAvalonia.Core.Domain.Packages;
using WinGetXAvalonia.Core.Events;
using WinGetXAvalonia.Core.Providers;
using WinGetXAvalonia.Core.State;
using WinGetXAvalonia.Helpers;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Core;
using WinGetXAvalonia.Services.Queue;
using PackageRequest = WinGetXAvalonia.Core.PackageEngine.PackageRequest;
using PackageOperation = WinGetXAvalonia.Core.PackageEngine.PackageOperation;
using WinGetXAvalonia.Services.Operations;

namespace WinGetXAvalonia.ViewModels;

public partial class InstalledViewModel : AsyncViewModel, IDisposable
{
    private readonly IPackageProvider _provider;
    private readonly IOperationQueue _queue;
    private readonly IEventBus _eventBus;
    private readonly ILauncherService _launcher;
    private readonly PackageCacheService _cache;

    private readonly List<PackageModel> _allPackages = new();
    private IDisposable? _subscription;
    private bool _refreshing;
    private CancellationTokenSource? _searchCts;

    public event EventHandler? BrowseRequested;

    public ObservableCollection<PackageModel> Packages { get; } = new();

    public int PackageCount => Packages.Count;

    public int SelectedCount => Packages.Count(x => x.Selected);

    [ObservableProperty]
    private string searchText = "";

    [ObservableProperty]
    private PackageSortMode selectedSort = PackageSortMode.Name;

    [ObservableProperty]
    private PackageFilterMode selectedFilter = PackageFilterMode.All;

    [ObservableProperty]
    private PackageModel? selectedPackage;

    [ObservableProperty]
    private PackageDetailsViewModel? details;

    partial void OnSelectedPackageChanged(PackageModel? value)
    {
        if (Details is not null)
            _ = Details.Load(value);
    }

    public InstalledViewModel(
        IPackageProvider provider,
        IOperationQueue queue,
        IEventBus eventBus,
        ILauncherService launcher,
        IOperationService operations,
        IClipboardService clipboard,
        PackageCacheService cache)
    {
        _provider = provider;
        _queue = queue;
        _eventBus = eventBus;
        _launcher = launcher;
        _cache = cache;

        Details = new PackageDetailsViewModel(provider, operations, clipboard, launcher);

        _subscription =
            _eventBus.Subscribe<PackageChangedEvent>(e =>
                Dispatcher.UIThread.Post(() => _ = Refresh()));

        Packages.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(PackageCount));
            OnPropertyChanged(nameof(SelectedCount));
        };
    }

    public override async Task InitializeAsync()
    {
        if (_refreshing)
            return;

        await Refresh();
    }

    partial void OnSearchTextChanged(string value)
    {
        _searchCts?.Cancel();
        _searchCts?.Dispose();

        _searchCts = new CancellationTokenSource();

        // her tuş basışında filtreyi çalıştırmayalım, 200ms bekleyelim
        DebounceSearch(_searchCts.Token).FireAndForget();
    }

    partial void OnSelectedSortChanged(PackageSortMode value)
        => ApplyFilter();

    partial void OnSelectedFilterChanged(PackageFilterMode value)
        => ApplyFilter();

    [RelayCommand]
    public async Task Refresh()
    {
        if (_refreshing)
            return;

        _refreshing = true;

        try
        {
            State = LoadState.Loading;
            ErrorMessage = null;

            var apps = await _provider.GetInstalledAsync();

            var availableVersions = await LoadAvailableVersionsAsync();

            _allPackages.Clear();

            foreach (var app in apps)
                _allPackages.Add(ToModel(app, availableVersions));

            ApplyFilter();

            if (SelectedPackage is null && Packages.Count > 0)
                SelectedPackage = Packages[0];

            if (SelectedPackage is not null)
            {
                SelectedPackage = Packages.FirstOrDefault(x =>
                    x.Id.Equals(SelectedPackage.Id,
                        StringComparison.OrdinalIgnoreCase));
            }

            State = _allPackages.Count == 0
                ? LoadState.Empty
                : LoadState.Loaded;

            _cache.Clear();
        }
        catch (Exception ex)
        {
            State = LoadState.Error;
            ErrorMessage = ex.Message;
        }
        finally
        {
            _refreshing = false;
        }
    }

    private async Task DebounceSearch(CancellationToken token)
    {
        try
        {
            await Task.Delay(200, token);

            ApplyFilter();
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task<Dictionary<string, string>?> LoadAvailableVersionsAsync()
    {
        try
        {
            return (await _provider.GetUpdatesAsync())
                .Where(p => !string.IsNullOrWhiteSpace(p.Id))
                .ToDictionary(p => p.Id, p => p.Available, StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            // güncelleme listesi alınamadıysa kartları boş çipleriyle gösterelim
            return null;
        }
    }

    [RelayCommand]
    private async Task Update(PackageModel package)
    {
        if (string.IsNullOrWhiteSpace(package.Id))
            return;

        await _queue.EnqueueAsync(new PackageRequest
        {
            PackageId = package.Id,
            Operation = PackageOperation.Update
        });
    }

    [RelayCommand]
    private async Task Remove(PackageModel? package)
    {
        if (package is null)
            return;

        if (SelectedPackage == package)
        {
            SelectedPackage = null;
            Details?.Clear();
        }

        Packages.Remove(package);
        _allPackages.Remove(package);
        Unsubscribe(package);

        await _queue.EnqueueAsync(new PackageRequest
        {
            PackageId = package.Id,
            Operation = PackageOperation.Remove
        });

        OnPropertyChanged(nameof(PackageCount));
        OnPropertyChanged(nameof(SelectedCount));
    }

    [RelayCommand]
    private void CopyId(PackageModel? package)
    {
        if (package is null)
            return;

        CurrentWindow?.Clipboard?.SetTextAsync(package.Id).FireAndForget();
    }

    [RelayCommand]
    private void CopyName(PackageModel? package)
    {
        if (package is null)
            return;

        CurrentWindow?.Clipboard?.SetTextAsync(package.Name).FireAndForget();
    }

    [RelayCommand]
    private void SelectAll()
    {
        foreach (var package in Packages)
            package.Selected = true;

        OnPropertyChanged(nameof(SelectedCount));
    }

    [RelayCommand]
    private async Task OpenHomepage(PackageModel? package)
    {
        if (package is null)
            return;

        var url = await _provider.GetHomepageAsync(package.Id);

        if (!string.IsNullOrWhiteSpace(url))
            await _launcher.OpenUrlAsync(url);
    }

    [RelayCommand]
    private void Browse()
        => BrowseRequested?.Invoke(this, EventArgs.Empty);

    [RelayCommand]
    private async Task RemoveSelected()
    {
        Details?.Clear();
        SelectedPackage = null;

        var selected = Packages
            .Where(x => x.Selected)
            .ToList();

        foreach (var package in selected)
        {
            await _queue.EnqueueAsync(new PackageRequest
            {
                PackageId = package.Id,
                Operation = PackageOperation.Remove
            });

            Packages.Remove(package);
            _allPackages.Remove(package);
            Unsubscribe(package);
        }

        OnPropertyChanged(nameof(PackageCount));
        OnPropertyChanged(nameof(SelectedCount));

        SelectedPackage = null;
    }

    [RelayCommand]
    private async Task UpdateSelected()
    {
        var selected = Packages
            .Where(x => x.Selected)
            .ToList();

        foreach (var package in selected)
            await _queue.EnqueueAsync(new PackageRequest
            {
                PackageId = package.Id,
                Operation = PackageOperation.Update
            });

        await Refresh();

        Details?.Clear();
        SelectedPackage = null;
    }

    private void ApplyFilter()
    {
        UnsubscribeAll();

        Packages.Clear();

        IEnumerable<PackageModel> filtered =
            PackageFilter.Apply(_allPackages, SearchText, SelectedFilter);

        filtered = PackageSorter.Sort(filtered, SelectedSort);

        foreach (var package in filtered)
        {
            Subscribe(package);
            Packages.Add(package);
        }

        OnPropertyChanged(nameof(PackageCount));
        OnPropertyChanged(nameof(SelectedCount));

        if (SelectedPackage is not null &&
            !Packages.Contains(SelectedPackage))
        {
            SelectedPackage = null;
            Details?.Clear();
        }
    }

    private void Subscribe(PackageModel package)
        => package.PropertyChanged += OnPackagePropertyChanged;

    private void Unsubscribe(PackageModel package)
        => package.PropertyChanged -= OnPackagePropertyChanged;

    private void UnsubscribeAll()
    {
        foreach (var package in Packages)
            Unsubscribe(package);
    }

    private void OnPackagePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PackageModel.Selected))
        {
            OnPropertyChanged(nameof(SelectedCount));
            return;
        }

        if (e.PropertyName == nameof(PackageModel.Name) ||
            e.PropertyName == nameof(PackageModel.Version))
        {
            ApplyFilter();
        }
    }

    private static Avalonia.Controls.Window? CurrentWindow
        => Application.Current?.ApplicationLifetime
            is IClassicDesktopStyleApplicationLifetime { MainWindow: { } window }
            ? window
            : null;

    private static PackageModel ToModel(
        InstalledPackage app,
        IReadOnlyDictionary<string, string>? availableVersions = null)
    {
        var model = new PackageModel
        {
            Icon = IconProvider.Get(app.Name),
            Name = app.Name,
            Id = app.Id,
            Version = app.Version,
            Source = app.Source
        };

        if (availableVersions is not null
            && !string.IsNullOrWhiteSpace(app.Id)
            && availableVersions.TryGetValue(app.Id, out var availableVersion))
            model.Available = availableVersion;

        return model;
    }

    public void Dispose()
    {
        _searchCts?.Cancel();
        _searchCts?.Dispose();

        _subscription?.Dispose();

        UnsubscribeAll();

        Details?.Clear();
    }
}

