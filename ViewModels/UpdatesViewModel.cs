// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using WinGetXAvalonia.Collections;
using WinGetXAvalonia.Core.Domain.Packages;
using WinGetXAvalonia.Core.Events;
using WinGetXAvalonia.Core.Providers;
using WinGetXAvalonia.Core.State;
using WinGetXAvalonia.Helpers;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Core;
using WinGetXAvalonia.Services.Operations;

namespace WinGetXAvalonia.ViewModels;

public partial class UpdatesViewModel : AsyncViewModel, IDisposable
{
    private readonly IPackageProvider _provider;
    private readonly IOperationService _operations;
    private readonly IEventBus _eventBus;
    private readonly ILauncherService _launcher;

    private readonly List<PackageModel> _allPackages = new();
    private IDisposable? _subscription;
    private bool _refreshing;
    private CancellationTokenSource? _searchCts;

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

    public ObservableRangeCollection<PackageModel> Updates { get; } = new();

    public int PackageCount => Updates.Count;

    public UpdatesViewModel(
        IPackageProvider provider,
        IOperationService operations,
        IEventBus eventBus,
        ILauncherService launcher,
        IClipboardService clipboard)
    {
        _provider = provider;
        _operations = operations;
        _eventBus = eventBus;
        _launcher = launcher;

        Details = new PackageDetailsViewModel(provider, operations, clipboard, launcher);

        _subscription =
            _eventBus.Subscribe<PackageChangedEvent>(e => _ = Refresh());

        Updates.CollectionChanged += (_, _) => OnPropertyChanged(nameof(PackageCount));
    }

    partial void OnSelectedPackageChanged(PackageModel? value)
    {
        if (Details is not null)
            _ = Details.Load(value);
    }

    public Task InitializeAsync()
        => Refresh();

    partial void OnSearchTextChanged(string value)
    {
        _searchCts?.Cancel();
        _searchCts?.Dispose();

        _searchCts = new CancellationTokenSource();

        // her tuş basışında filtreyi çalıştırmayalım, 200ms bekleyelim
        _ = DebounceSearch(_searchCts.Token);
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

            var updates = await _provider.GetUpdatesAsync();

            _allPackages.Clear();

            foreach (var update in updates)
                _allPackages.Add(ToModel(update));

            ApplyFilter();

            State = _allPackages.Count == 0 ? LoadState.Empty : LoadState.Loaded;
        }
        catch (Exception)
        {
            State = LoadState.Error;
            ErrorMessage = "Failed to load updates.";
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

    [RelayCommand]
    private async Task UpgradeAll()
    {
        await _operations.UpgradeAllAsync();
        await Refresh();
    }

    [RelayCommand]
    private async Task Update(PackageModel? package)
    {
        if (package is null || string.IsNullOrWhiteSpace(package.Id))
            return;

        await _operations.UpdateAsync(package.Id);

        await Refresh();
    }

    [RelayCommand]
    private void CopyId(PackageModel? package)
    {
        if (package is null || string.IsNullOrWhiteSpace(package.Id))
            return;

        _ = CurrentWindow?.Clipboard?.SetTextAsync(package.Id);
    }

    [RelayCommand]
    private void CopyName(PackageModel? package)
    {
        if (package is null || string.IsNullOrWhiteSpace(package.Name))
            return;

        _ = CurrentWindow?.Clipboard?.SetTextAsync(package.Name);
    }

    [RelayCommand]
    private async Task OpenHomepage(PackageModel? package)
    {
        if (package is null || string.IsNullOrWhiteSpace(package.Id))
            return;

        var url = await _provider.GetHomepageAsync(package.Id);

        if (!string.IsNullOrWhiteSpace(url))
            await _launcher.OpenUrlAsync(url);
    }

    private void ApplyFilter()
    {
        IEnumerable<PackageModel> filtered =
            PackageFilter.Apply(_allPackages, SearchText, SelectedFilter);

        Updates.ReplaceAll(PackageSorter.Sort(filtered, SelectedSort));
    }

    private static Avalonia.Controls.Window? CurrentWindow
        => Application.Current?.ApplicationLifetime
            is IClassicDesktopStyleApplicationLifetime { MainWindow: { } window }
            ? window
            : null;

    private static PackageModel ToModel(UpdatePackage update)
        => new()
        {
            Icon = IconProvider.Get(update.Name),
            Name = update.Name,
            Id = update.Id,
            Version = update.Version,
            Available = update.Available,
            Source = update.Source
        };

    public void Dispose()
    {
        _subscription?.Dispose();
        _subscription = null;
    }
}
