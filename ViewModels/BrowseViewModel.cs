// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using WinGetXAvalonia.Collections;
using WinGetXAvalonia.Core.Domain.Packages;
using WinGetXAvalonia.Core.Providers;
using WinGetXAvalonia.Core.State;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Core;
using WinGetXAvalonia.Services.Operations;

namespace WinGetXAvalonia.ViewModels;

public partial class BrowseViewModel : AsyncViewModel
{
    private readonly IPackageProvider _provider;
    private readonly IOperationService _operations;
    private readonly PackageSearchCache _cache;
    private readonly PackageCacheService _detailsCache;
    private readonly IFeaturedCatalog _featured;
    private readonly IClipboardService _clipboard;
    private readonly ILauncherService _launcher;
    private readonly LoggerService? _logger;

    private Task? _initializationTask;
    private CancellationTokenSource? _searchCts;
    private CancellationTokenSource? _suggestCts;
    private PackageModel? _lastSelected;
    private PackageIndex? _index;

    [ObservableProperty]
    private string searchText = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EmptySearchTitle))]
    [NotifyPropertyChangedFor(nameof(EmptySearchDescription))]
    private string? lastQuery;

    [ObservableProperty]
    private string? selectedSuggestion;

    [ObservableProperty]
    private PackageModel? selectedPackage;

    public PackageDetailsViewModel SelectedDetails { get; }

    public ObservableCollection<PackageModel> Featured { get; } = new();

    public ObservableCollection<PackageCategory> Categories { get; } = new();

    public ObservableRangeCollection<PackageModel> Results { get; } = new();

    public ObservableRangeCollection<string> Suggestions { get; } = new();

    public string ResultsSummary => State switch
    {
        LoadState.None => string.Empty,
        LoadState.Loading => string.Empty,
        LoadState.Error => string.Empty,
        LoadState.Empty => "No packages found",
        _ => Results.Count switch
        {
            0 => "No packages found",
            1 => "1 package found",
            var count => $"{count} packages found"
        }
    };

    public string EmptySearchTitle =>
        string.IsNullOrWhiteSpace(LastQuery)
            ? "No packages found"
            : $"No results for \"{LastQuery}\"";

    public string EmptySearchDescription =>
        string.IsNullOrWhiteSpace(LastQuery)
            ? "Try a different package name or category."
            : "Check the spelling, try a shorter name, or browse a category instead.";

    public BrowseViewModel(
        IPackageProvider provider,
        IOperationService operations,
        PackageSearchCache cache,
        PackageCacheService detailsCache,
        IFeaturedCatalog featured,
        IClipboardService clipboard,
        ILauncherService launcher,
        LoggerService? logger = null)
    {
        _provider = provider;
        _operations = operations;
        _cache = cache;
        _detailsCache = detailsCache;
        _featured = featured;
        _clipboard = clipboard;
        _launcher = launcher;
        _logger = logger;

        SelectedDetails = new PackageDetailsViewModel(provider, operations, clipboard, launcher);

        SeedCategories();

        Results.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(ResultsSummary));
        };

        PropertyChanged += (_, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(IsInitial):
                case nameof(IsLoading):
                case nameof(IsEmpty):
                case nameof(HasError):
                case nameof(IsLoaded):
                    OnPropertyChanged(nameof(ResultsSummary));
                    break;
            }
        };
    }

    public override Task InitializeAsync() =>
        _initializationTask ??= InitializeCoreAsync();

    private async Task InitializeCoreAsync()
    {
        State = LoadState.Loading;

        try
        {
            await LoadFeaturedAsync();

            if (State != LoadState.Loaded && State != LoadState.Empty && State != LoadState.Error)
            {
                State = Results.Count > 0 ? LoadState.Loaded : LoadState.Empty;
            }
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "Browse initialization failed");
            State = LoadState.Error;
            ErrorMessage = "Failed to load featured packages.";
        }
    }

    partial void OnSearchTextChanged(string value)
        => RefreshSuggestionsAsync(value).FireAndForget(_logger, "Browse suggestions");

    partial void OnSelectedSuggestionChanged(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        SearchText = value;
        SearchCommand.Execute(null);
    }

    partial void OnSelectedPackageChanged(PackageModel? value)
    {
        if (_lastSelected is not null)
            _lastSelected.Selected = false;

        if (value is not null)
        {
            value.Selected = true;
            _lastSelected = value;
        }
        else
        {
            _lastSelected = null;
            SelectedDetails.Clear();
        }

        _ = SelectedDetails.Load(value);
    }

    private AsyncRelayCommand? _searchCommand;
    private RelayCommand? _clearSearchCommand;
    private AsyncRelayCommand<PackageCategory?>? _selectCategoryCommand;
    private AsyncRelayCommand<PackageModel?>? _installCommand;
    private AsyncRelayCommand<PackageModel?>? _updateCommand;
    private AsyncRelayCommand<PackageDetailsViewModel?>? _openHomepageCommand;
    private RelayCommand<PackageModel?>? _copyIdCommand;
    private RelayCommand<PackageModel?>? _copyNameCommand;
    private AsyncRelayCommand<PackageModel?>? _installOrUpdateCommand;

    public AsyncRelayCommand SearchCommand => _searchCommand ??= new AsyncRelayCommand(SearchInternal);
    public RelayCommand ClearSearchCommand => _clearSearchCommand ??= new RelayCommand(ClearSearchInternal);
    public AsyncRelayCommand<PackageCategory?> SelectCategoryCommand => _selectCategoryCommand ??= new AsyncRelayCommand<PackageCategory?>(SelectCategoryInternal);
    public AsyncRelayCommand<PackageModel?> InstallCommand => _installCommand ??= new AsyncRelayCommand<PackageModel?>(InstallInternal);
    public AsyncRelayCommand<PackageModel?> UpdateCommand => _updateCommand ??= new AsyncRelayCommand<PackageModel?>(UpdateInternal);
    public AsyncRelayCommand<PackageDetailsViewModel?> OpenHomepageCommand => _openHomepageCommand ??= new AsyncRelayCommand<PackageDetailsViewModel?>(OpenHomepageInternal);
    public RelayCommand<PackageModel?> CopyIdCommand => _copyIdCommand ??= new RelayCommand<PackageModel?>(async (package) => await CopyIdInternal(package));
    public RelayCommand<PackageModel?> CopyNameCommand => _copyNameCommand ??= new RelayCommand<PackageModel?>(async (package) => await CopyNameInternal(package));
    public AsyncRelayCommand<PackageModel?> InstallOrUpdateCommand => _installOrUpdateCommand ??= new AsyncRelayCommand<PackageModel?>(InstallOrUpdateInternal);

    public async Task Search()
    {
        await SearchInternal();
    }

    public void ClearSearch()
    {
        ClearSearchInternal();
    }

    public async Task SelectCategory(PackageCategory? category)
    {
        await SelectCategoryInternal(category);
    }

    public async Task Install(PackageModel? package)
    {
        await InstallInternal(package);
    }

    public async Task Update(PackageModel? package)
    {
        await UpdateInternal(package);
    }

    public async Task OpenHomepage(PackageDetailsViewModel? details)
    {
        await OpenHomepageInternal(details);
    }

    public async Task CopyId(PackageModel? package)
    {
        await CopyIdInternal(package);
    }

    public async Task CopyName(PackageModel? package)
    {
        await CopyNameInternal(package);
    }

    public async Task InstallOrUpdate(PackageModel? package)
    {
        await InstallOrUpdateInternal(package);
    }

    private async Task SearchInternal()
    {
        SearchText = SearchText.Trim();

        if (string.IsNullOrWhiteSpace(SearchText))
            return;

        _searchCts?.Cancel();
        _searchCts?.Dispose();

        _searchCts = new CancellationTokenSource();

        await RunSearchAsync(SearchText, _searchCts.Token);
    }

    private void ClearSearchInternal()
    {
        _suggestCts?.Cancel();
        _suggestCts?.Dispose();
        _suggestCts = null;

        SearchText = "";
        LastQuery = null;
        SelectedSuggestion = null;
        Suggestions.Clear();
        Results.Clear();
        SelectedPackage = null;

        State = LoadState.None;
        ErrorMessage = null;

        OnPropertyChanged(nameof(ResultsSummary));

        SelectedDetails.Clear();

        _detailsCache.Clear();

        _lastSelected = null;
    }

    private async Task SelectCategoryInternal(PackageCategory? category)
    {
        if (category is null || string.IsNullOrWhiteSpace(category.Query))
            return;

        SearchText = category.Query;

        await RunSearchAsync(category.Query);
    }

    private async Task InstallInternal(PackageModel? package)
    {
        if (package is null || string.IsNullOrWhiteSpace(package.Id))
            return;

        await _operations.InstallAsync(package.Id);

        await SelectedDetails.Load(package);
    }

    private async Task UpdateInternal(PackageModel? package)
    {
        if (package is null || string.IsNullOrWhiteSpace(package.Id))
            return;

        await _operations.UpdateAsync(package.Id);

        await SelectedDetails.Load(package);
    }

    private async Task OpenHomepageInternal(PackageDetailsViewModel? details)
    {
        if (details is null || !details.HasHomepage)
            return;

        await _launcher.OpenUrlAsync(details.HomepageDisplay);
    }

    private async Task CopyIdInternal(PackageModel? package)
    {
        if (package is null || string.IsNullOrWhiteSpace(package.Id))
            return;

        await _clipboard.CopyTextAsync(package.Id);
    }

    private async Task CopyNameInternal(PackageModel? package)
    {
        if (package is null || string.IsNullOrWhiteSpace(package.Name))
            return;

        await _clipboard.CopyTextAsync(package.Name);
    }

    private async Task InstallOrUpdateInternal(PackageModel? package)
    {
        if (package is null || string.IsNullOrWhiteSpace(package.Id))
            return;

        if (package.InstallState == PackageInstallState.NotInstalled)
        {
            package.InstallState = PackageInstallState.Installing;

            try
            {
                await _operations.InstallAsync(package.Id);
            }
            catch
            {
                package.InstallState = PackageInstallState.NotInstalled;
                throw;
            }

            package.InstallState = PackageInstallState.Installed;
        }
        else if (package.InstallState == PackageInstallState.Installed && package.HasUpdate)
        {
            package.InstallState = PackageInstallState.Updating;

            try
            {
                await _operations.UpdateAsync(package.Id);
            }
            catch
            {
                package.InstallState = PackageInstallState.Installed;
                throw;
            }

            package.Available = "";
            package.InstallState = PackageInstallState.Installed;
        }

        await SelectedDetails.Load(package);
    }

    private async Task LoadFeaturedAsync()
    {
        IReadOnlyList<FeaturedItem> items;

        try
        {
            items = await _featured.GetAsync() ?? Array.Empty<FeaturedItem>();
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "Failed to load featured catalog");
            State = LoadState.Error;
            ErrorMessage = "Failed to load featured packages.";
            return;
        }

        using var gate = new SemaphoreSlim(4);

        var packages = await Task.WhenAll(items.Select(async item =>
        {
            await gate.WaitAsync();

            try
            {
                return await LoadFeaturedItemAsync(item);
            }
            finally
            {
                gate.Release();
            }
        }));

        foreach (var package in packages)
        {
            if (package is not null &&
                Featured.All(x => x.Id != package.Id))
            {
                Featured.Add(package);
            }
        }

        // After featured items are loaded, determine terminal state
        if (State == LoadState.Loading)
        {
            State = Featured.Count > 0 ? LoadState.Loaded : LoadState.Empty;
        }
    }

    private async Task<PackageModel?> LoadFeaturedItemAsync(FeaturedItem item)
    {
        try
        {
            var results = await GetOrSearchAsync(item.Id);

            var package = PackageMatcher.FindPreferred(results, item.Id);

            if (package is null)
                return null;

            await EnsureEnrichmentAsync();
            PackageStateCalculator.Apply(package, _index);

            package.IsFeatured = true;
            package.Description = item.Description;

            if (!string.IsNullOrWhiteSpace(item.Title))
                package.Name = item.Title;

            return package;
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, $"Failed to load featured item '{item.Id}'");
            return null;
        }
    }

    private async Task RunSearchAsync(string query, CancellationToken token = default)
    {
        State = LoadState.Loading;
        ErrorMessage = null;
        LastQuery = query;
        Results.Clear();

        try
        {
            var results = SearchEngine.Rank(
                await GetOrSearchAsync(query),
                query,
                new SearchOptions(UpdateAvailableFirst: true, FeaturedBoost: true));

            await EnsureEnrichmentAsync();

            var enriched = new List<PackageModel>(results.Count);

            foreach (var package in results)
            {
                token.ThrowIfCancellationRequested();

                PackageStateCalculator.Apply(package, _index);
                enriched.Add(package);
            }

            token.ThrowIfCancellationRequested();

            Results.ReplaceAll(enriched);

            if (Results.Count > 0)
            {
                SelectedPackage = Results[0];
            }

            State = Results.Count == 0 ? LoadState.Empty : LoadState.Loaded;

            _detailsCache.Clear();
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            SelectedPackage = null;
            SelectedDetails.Clear();

            State = LoadState.Error;
            ErrorMessage = ex.Message;
        }

        OnPropertyChanged(nameof(ResultsSummary));
    }

    private async Task<IReadOnlyList<PackageModel>> GetOrSearchAsync(string query)
    {
        if (_cache.TryGet(query, out var cached))
            return cached!;

        var results = await _provider.SearchAsync(query);

        _cache.Set(query, results);

        return results;
    }

    private async Task RefreshSuggestionsAsync(string value)
    {
        _suggestCts?.Cancel();
        _suggestCts?.Dispose();

        var cts = _suggestCts = new CancellationTokenSource();

        if (string.IsNullOrWhiteSpace(value) || value.Length < 2)
        {
            Suggestions.Clear();
            return;
        }

        try
        {
            await Task.Delay(250, cts.Token);

            var names = await GetSuggestionNamesAsync(value, cts.Token);

            if (cts.Token.IsCancellationRequested)
                return;

            Suggestions.ReplaceAll(names);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "Failed to refresh search suggestions");
        }
    }

    private async Task<IReadOnlyList<string>> GetSuggestionNamesAsync(string text, CancellationToken token)
    {
        var matches = SearchEngine.Rank(
                _cache.All().Where(p => p.Name.Contains(text, StringComparison.OrdinalIgnoreCase)),
                text)
            .Select(p => p.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(8)
            .ToList();

        if (matches.Count > 0)
            return matches;

        var results = await GetOrSearchAsync(text);

        token.ThrowIfCancellationRequested();

        return results
            .Select(p => p.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(8)
            .ToList();
    }

    private async Task EnsureEnrichmentAsync()
    {
        if (_index is not null)
            return;

        var index = new PackageIndex();

        try
        {
            index.AddInstalled(await _provider.GetInstalledAsync());
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "Failed to load installed index for enrichment");
        }

        try
        {
            index.AddUpdates(await _provider.GetUpdatesAsync());
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "Failed to load updates index for enrichment");
        }

        _index = index;
    }

    private void SeedCategories()
    {
        Categories.Add(new() { Name = "Development", Query = "vscode", Icon = "developer" });
        Categories.Add(new() { Name = "Browsers", Query = "chrome", Icon = "source" });
        Categories.Add(new() { Name = "Media", Query = "vlc", Icon = "studio" });
        Categories.Add(new() { Name = "Utilities", Query = "powertoys", Icon = "tool" });
        Categories.Add(new() { Name = "Gaming", Query = "steam", Icon = "automation" });
        Categories.Add(new() { Name = "Design", Query = "figma", Icon = "studio" });
    }
}
