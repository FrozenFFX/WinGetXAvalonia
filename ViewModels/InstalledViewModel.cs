using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
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
using WinGetXAvalonia.Services.Operations;

namespace WinGetXAvalonia.ViewModels;

public partial class InstalledViewModel : AsyncViewModel
{
    private readonly IPackageProvider _provider;
    private readonly IOperationService _operations;
    private readonly IEventBus _eventBus;
    private readonly ILauncherService _launcher;

    private readonly List<PackageModel> _allPackages = new();
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

    public InstalledViewModel(
        IPackageProvider provider,
        IOperationService operations,
        IEventBus eventBus,
        ILauncherService launcher)
    {
        _provider = provider;
        _operations = operations;
        _eventBus = eventBus;
        _launcher = launcher;

        _eventBus.Subscribe<PackageChangedEvent>(e => _ = Refresh());
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

            var apps = await _provider.GetInstalledAsync();

            var availableVersions = await LoadAvailableVersionsAsync();

            _allPackages.Clear();

            foreach (var app in apps)
                _allPackages.Add(ToModel(app, availableVersions));

            ApplyFilter();

            State = _allPackages.Count == 0 ? LoadState.Empty : LoadState.Loaded;
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

        await _operations.UpdateAsync(package.Id);
    }

    [RelayCommand]
    private async Task Remove(PackageModel package)
    {
        if (package is null)
            return;

        Packages.Remove(package);
        _allPackages.Remove(package);
        Unsubscribe(package);

        await _operations.RemoveAsync(package.Id);

        OnPropertyChanged(nameof(PackageCount));
        OnPropertyChanged(nameof(SelectedCount));
    }

    [RelayCommand]
    private void CopyId(PackageModel package)
    {
        if (package is null)
            return;

        _ = CurrentWindow?.Clipboard?.SetTextAsync(package.Id);
    }

    [RelayCommand]
    private void CopyName(PackageModel package)
    {
        if (package is null)
            return;

        _ = CurrentWindow?.Clipboard?.SetTextAsync(package.Name);
    }

    [RelayCommand]
    private async Task OpenHomepage(PackageModel package)
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
        var selected = Packages
            .Where(x => x.Selected)
            .ToList();

        foreach (var package in selected)
        {
            await _operations.RemoveAsync(package.Id);

            Packages.Remove(package);
            _allPackages.Remove(package);
            Unsubscribe(package);
        }

        OnPropertyChanged(nameof(PackageCount));
        OnPropertyChanged(nameof(SelectedCount));
    }

    [RelayCommand]
    private async Task UpdateSelected()
    {
        var selected = Packages
            .Where(x => x.Selected)
            .ToList();

        foreach (var package in selected)
            await _operations.UpdateAsync(package.Id);

        await Refresh();
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
            OnPropertyChanged(nameof(SelectedCount));
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
}
