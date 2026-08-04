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
    private readonly IFeaturedCatalog _featured;
    private readonly IClipboardService _clipboard;
    private readonly ILauncherService _launcher;

    private bool _initialized;
    private CancellationTokenSource? _suggestCts;
    private PackageModel? _lastSelected;
    private PackageIndex? _index;

    [ObservableProperty]
    private string searchText = "";

    [ObservableProperty]
    private string? selectedSuggestion;

    [ObservableProperty]
    private PackageModel? selectedPackage;

    public PackageDetailsViewModel SelectedDetails { get; }

    public ObservableCollection<PackageModel> Featured { get; } = new();

    public ObservableCollection<PackageCategory> Categories { get; } = new();

    public ObservableRangeCollection<PackageModel> Results { get; } = new();

    public ObservableRangeCollection<string> Suggestions { get; } = new();

    public string ResultsSummary => Results.Count switch
    {
        0 => "No packages found",
        1 => "1 package found",
        var count => $"{count} packages found"
    };

    public BrowseViewModel(
        IPackageProvider provider,
        IOperationService operations,
        PackageSearchCache cache,
        IFeaturedCatalog featured,
        IClipboardService clipboard,
        ILauncherService launcher)
    {
        _provider = provider;
        _operations = operations;
        _cache = cache;
        _featured = featured;
        _clipboard = clipboard;
        _launcher = launcher;

        SelectedDetails = new PackageDetailsViewModel(provider, operations, clipboard, launcher);

        SeedCategories();
    }

    public async Task InitializeAsync()
    {
        if (_initialized)
            return;

        _initialized = true;

        await LoadFeaturedAsync();
    }

    partial void OnSearchTextChanged(string value)
        => _ = RefreshSuggestionsAsync(value);

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

        _ = SelectedDetails.Load(value);
    }

    [RelayCommand]
    public async Task Search()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
            return;

        await RunSearchAsync(SearchText);
    }

    [RelayCommand]
    private void ClearSearch()
    {
        _suggestCts?.Cancel();
        _suggestCts?.Dispose();
        _suggestCts = null;

        SearchText = "";
        SelectedSuggestion = null;
        Suggestions.Clear();
        Results.Clear();
        SelectedPackage = null;

        State = LoadState.None;
        ErrorMessage = null;

        OnPropertyChanged(nameof(ResultsSummary));
    }

    [RelayCommand]
    private async Task SelectCategory(PackageCategory? category)
    {
        if (category is null || string.IsNullOrWhiteSpace(category.Query))
            return;

        SearchText = category.Query;

        await RunSearchAsync(category.Query);
    }

    [RelayCommand]
    private async Task Install(PackageModel? package)
    {
        if (package is null || string.IsNullOrWhiteSpace(package.Id))
            return;

        await _operations.InstallAsync(package.Id);
    }

    [RelayCommand]
    private async Task Update(PackageModel? package)
    {
        if (package is null || string.IsNullOrWhiteSpace(package.Id))
            return;

        await _operations.UpdateAsync(package.Id);
    }

    [RelayCommand]
    private async Task OpenHomepage(PackageDetailsViewModel? details)
    {
        if (details is null || !details.HasHomepage)
            return;

        await _launcher.OpenUrlAsync(details.HomepageDisplay);
    }

    [RelayCommand]
    private async Task CopyId(PackageModel? package)
    {
        if (package is null || string.IsNullOrWhiteSpace(package.Id))
            return;

        await _clipboard.CopyTextAsync(package.Id);
    }

    [RelayCommand]
    private async Task CopyName(PackageModel? package)
    {
        if (package is null || string.IsNullOrWhiteSpace(package.Name))
            return;

        await _clipboard.CopyTextAsync(package.Name);
    }

    [RelayCommand]
    private async Task InstallOrUpdate(PackageModel? package)
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
    }

    private async Task LoadFeaturedAsync()
    {
        IReadOnlyList<FeaturedItem> items;

        try
        {
            items = await _featured.GetAsync();
        }
        catch
        {
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
            if (package is not null)
                Featured.Add(package);
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
        catch
        {
            // tek bir paket çökmüşse listeyi komple bozmayalım
            return null;
        }
    }

    private async Task RunSearchAsync(string query)
    {
        State = LoadState.Loading;
        ErrorMessage = null;
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
                PackageStateCalculator.Apply(package, _index);
                enriched.Add(package);
            }

            Results.ReplaceAll(enriched);

            SelectedPackage = null;

            State = Results.Count == 0 ? LoadState.Empty : LoadState.Loaded;
        }
        catch (Exception ex)
        {
            State = LoadState.Error;
            ErrorMessage = ex.Message;
        }

        OnPropertyChanged(nameof(ResultsSummary));
    }

    private async Task<IReadOnlyList<PackageModel>> GetOrSearchAsync(string query)
    {
        // aynı sorguyu tekrar tekrar winget'e sormayalım
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
        catch
        {
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
        // kurulu sürüm ve güncelleme bilgisi bir kez çekilir, sonra kartlara işlenir
        if (_index is not null)
            return;

        var index = new PackageIndex();

        try
        {
            index.AddInstalled(await _provider.GetInstalledAsync());
        }
        catch
        {
        }

        try
        {
            index.AddUpdates(await _provider.GetUpdatesAsync());
        }
        catch
        {
        }

        _index = index;
    }

    private void SeedCategories()
    {
        Categories.Add(new() { Name = "Development", Query = "git vscode docker", Icon = "💻" });
        Categories.Add(new() { Name = "Browsers", Query = "chrome firefox edge", Icon = "🌐" });
        Categories.Add(new() { Name = "Media", Query = "vlc spotify", Icon = "🎬" });
        Categories.Add(new() { Name = "Utilities", Query = "powertoys 7zip", Icon = "🛠" });
        Categories.Add(new() { Name = "Gaming", Query = "steam", Icon = "🎮" });
        Categories.Add(new() { Name = "Design", Query = "figma", Icon = "🎨" });
    }
}
