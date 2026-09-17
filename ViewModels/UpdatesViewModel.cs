// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using WinGetXAvalonia.Collections;
using WinGetXAvalonia.Core.Domain.Packages;
using WinGetXAvalonia.Core.Events;
using WinGetXAvalonia.Core.Providers;
using WinGetXAvalonia.Core.State;
using WinGetXAvalonia.Helpers;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Core;
using WinGetXAvalonia.Services.Operations;
using WinGetXAvalonia.Services.Updates;

namespace WinGetXAvalonia.ViewModels;

public partial class UpdatesViewModel : AsyncViewModel, IDisposable
{
    private readonly IPackageProvider _provider;
    private readonly IOperationService _operations;
    private readonly IUpdateService _updates;
    private readonly IEventBus _eventBus;
    private readonly ILauncherService _launcher;
    private readonly PackageCacheService _cache;

    private readonly List<PackageModel> _allPackages = new();
    private readonly HashSet<string> _selectedIds = new(StringComparer.OrdinalIgnoreCase);
    private IDisposable? _subscription;
    private bool _refreshing;
    private bool _bulkUpdating;
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

    public int SelectedCount => Updates.Count(p => p.Selected);

    public bool HasSelection => SelectedCount > 0;

    public ObservableRangeCollection<PackageModel> Updates { get; } = new();

    public int PackageCount => Updates.Count;

    public UpdatesViewModel(
        IPackageProvider provider,
        IOperationService operations,
        IUpdateService updates,
        IEventBus eventBus,
        ILauncherService launcher,
        IClipboardService clipboard,
        PackageCacheService cache)
    {
        _provider = provider;
        _operations = operations;
        _updates = updates;
        _eventBus = eventBus;
        _launcher = launcher;
        _cache = cache;

        Details = new PackageDetailsViewModel(provider, operations, clipboard, launcher);

        _subscription =
            _eventBus.Subscribe<PackageChangedEvent>(e =>
                Dispatcher.UIThread.Post(() => _ = Refresh()));

        Updates.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(PackageCount));
        };
    }

    partial void OnSelectedPackageChanged(PackageModel? value)
        => _ = Details?.Load(value);

    [RelayCommand]
    private void SelectAll()
    {
        foreach (var item in Updates)
            item.Selected = true;

        SyncSelectionIds();
    }

    [RelayCommand]
    private void DeselectAll()
    {
        foreach (var item in Updates)
            item.Selected = false;

        _selectedIds.Clear();
        RaiseSelectionChanged();
    }

    [RelayCommand(CanExecute = nameof(CanUpdateSelected))]
    private async Task UpdateSelected()
    {
        if (!HasSelection)
            return;

        _bulkUpdating = true;
        UpdateSelectedCommand.NotifyCanExecuteChanged();

        try
        {
            var ids = Updates
                .Where(p => p.Selected && !string.IsNullOrWhiteSpace(p.Id))
                .Select(p => p.Id)
                .ToList();

            foreach (var id in ids)
            {
                var live = Updates.FirstOrDefault(p =>
                    p.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

                if (live is not null)
                    live.InstallState = PackageInstallState.Updating;

                await _operations.UpdateAsync(id);
            }

            foreach (var item in Updates)
                item.Selected = false;

            _selectedIds.Clear();
        }
        finally
        {
            _updates.Invalidate();
            _bulkUpdating = false;
            UpdateSelectedCommand.NotifyCanExecuteChanged();
            RaiseSelectionChanged();
        }

        await Refresh();

        Details?.Clear();
    }

    private bool CanUpdateSelected()
        => HasSelection && !_bulkUpdating;

    public override Task InitializeAsync()
        => Refresh();

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

            var updates = await _updates.GetUpdatesAsync();

            _allPackages.Clear();

            foreach (var update in updates)
                _allPackages.Add(ToModel(update));

            foreach (var model in _allPackages)
                model.Selected = _selectedIds.Contains(model.Id);

            ApplyFilter();

            SyncSelectionIds();

            if (SelectedPackage is null && Updates.Count > 0)
                SelectedPackage = Updates[0];

            if (SelectedPackage is not null)
            {
                SelectedPackage = Updates.FirstOrDefault(x =>
                    x.Id.Equals(
                        SelectedPackage.Id,
                        StringComparison.OrdinalIgnoreCase));
            }

            State = _allPackages.Count == 0 ? LoadState.Empty : LoadState.Loaded;

            _cache.Clear();
        }
        catch (Exception)
        {
            Details?.Clear();

            SelectedPackage = null;

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

    [RelayCommand(CanExecute = nameof(CanUpgradeAll))]
    private async Task UpgradeAll()
    {
        _bulkUpdating = true;
        UpgradeAllCommand.NotifyCanExecuteChanged();

        try
        {
            await _operations.UpgradeAllAsync();

            _updates.Invalidate();
        }
        finally
        {
            _bulkUpdating = false;
            UpgradeAllCommand.NotifyCanExecuteChanged();
            RaiseSelectionChanged();
        }

        await Refresh();

        Details?.Clear();

        SelectedPackage = null;
    }

    private bool CanUpgradeAll()
        => !_bulkUpdating;

    [RelayCommand]
    private async Task Update(PackageModel? package)
    {
        if (package is null || string.IsNullOrWhiteSpace(package.Id))
            return;

        package.InstallState = PackageInstallState.Updating;

        await _operations.UpdateAsync(package.Id);

        _updates.Invalidate();

        await Refresh();

        var current = Updates.FirstOrDefault(x =>
            x.Id.Equals(package.Id, StringComparison.OrdinalIgnoreCase));

        await Details!.Load(current ?? package);
    }

    [RelayCommand]
    private void CopyId(PackageModel? package)
    {
        if (package is null || string.IsNullOrWhiteSpace(package.Id))
            return;

        CurrentWindow?.Clipboard?.SetTextAsync(package.Id).FireAndForget();
    }

    [RelayCommand]
    private void CopyName(PackageModel? package)
    {
        if (package is null || string.IsNullOrWhiteSpace(package.Name))
            return;

        CurrentWindow?.Clipboard?.SetTextAsync(package.Name).FireAndForget();
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
            PackageFilter.Apply(
                _allPackages,
                SearchText.Trim(),
                SelectedFilter);

        Updates.ReplaceAll(
            PackageSorter.Sort(filtered, SelectedSort));

        if (SelectedPackage is not null &&
            !Updates.Contains(SelectedPackage))
        {
            SelectedPackage = null;
            Details?.Clear();
        }

        RaiseSelectionChanged();
    }

    private void SyncSelectionIds()
    {
        var retained = new List<string>();

        foreach (var item in _allPackages)
        {
            if (item.Selected)
                retained.Add(item.Id);
        }

        foreach (var id in _selectedIds)
        {
            if (string.IsNullOrWhiteSpace(id)) continue;

            if (!retained.Contains(id, StringComparer.OrdinalIgnoreCase)
                && _allPackages.Any(p => p.Id.Equals(id, StringComparison.OrdinalIgnoreCase)))
            {
                retained.Add(id);
            }
        }

        _selectedIds.Clear();

        foreach (var id in retained)
            _selectedIds.Add(id);

        RaiseSelectionChanged();
    }

    private void RaiseSelectionChanged()
    {
        OnPropertyChanged(nameof(SelectedCount));
        OnPropertyChanged(nameof(HasSelection));
        UpdateSelectedCommand.NotifyCanExecuteChanged();
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
        _searchCts?.Cancel();
        _searchCts?.Dispose();

        _subscription?.Dispose();

        Details?.Clear();
    }
}

