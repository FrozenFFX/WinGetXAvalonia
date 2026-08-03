// ==========================================================
// WinGetX - https://github.com/FrozenFFX
// ==========================================================
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using WinGetXAvalonia.Core.Providers;
using WinGetXAvalonia.Core.State;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Core;
using WinGetXAvalonia.Services.Operations;

namespace WinGetXAvalonia.ViewModels;

// kaneki
public partial class BrowseViewModel : AsyncViewModel
{
    private readonly IPackageProvider _provider;
    private readonly IOperationService _operations;
    private readonly PackageSearchCache _cache;
    private readonly IFeaturedCatalog _featured;

    private bool _initialized;
    private CancellationTokenSource? _suggestCts;

    private Dictionary<string, string>? _installedVersions;
    private Dictionary<string, string>? _availableUpdates;

    [ObservableProperty]
    private string searchText = "";

    [ObservableProperty]
    private PackageModel? selectedPackage;

    public ObservableCollection<PackageModel> Featured { get; } = new();

    public ObservableCollection<PackageCategory> Categories { get; } = new();

    public ObservableCollection<PackageModel> Results { get; } = new();

    public ObservableCollection<string> Suggestions { get; } = new();

    public BrowseViewModel(
        IPackageProvider provider,
        IOperationService operations,
        PackageSearchCache cache,
        IFeaturedCatalog featured)
    {
        _provider = provider;
        _operations = operations;
        _cache = cache;
        _featured = featured;

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

    [RelayCommand]
    public async Task Search()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
            return;

        await RunSearchAsync(SearchText);
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

        await Task.WhenAll(items.Select(LoadFeaturedItemAsync));
    }

    private async Task LoadFeaturedItemAsync(FeaturedItem item)
    {
        try
        {
            var results = await GetOrSearchAsync(item.Id);

            var package = results.FirstOrDefault(p =>
                string.Equals(p.Id, item.Id, StringComparison.OrdinalIgnoreCase))
                ?? results.FirstOrDefault();

            if (package is null)
                return;

            await EnsureEnrichmentAsync();
            Enrich(package);

            if (!string.IsNullOrWhiteSpace(item.Title))
                package.Name = item.Title;

            Featured.Add(package);
        }
        catch
        {
            // A single failed featured package should not break the list.
        }
    }

    private async Task RunSearchAsync(string query)
    {
        State = LoadState.Loading;
        ErrorMessage = null;

        try
        {
            var results = await GetOrSearchAsync(query);

            await EnsureEnrichmentAsync();

            Results.Clear();

            foreach (var package in results)
            {
                Enrich(package);
                Results.Add(package);
            }

            SelectedPackage = null;

            State = Results.Count == 0 ? LoadState.Empty : LoadState.Loaded;
        }
        catch (Exception ex)
        {
            State = LoadState.Error;
            ErrorMessage = ex.Message;
        }
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

            Suggestions.Clear();

            foreach (string name in names)
                Suggestions.Add(name);
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
        var matches = _cache.All()
            .Where(p => p.Name.Contains(text, StringComparison.OrdinalIgnoreCase))
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
        if (_installedVersions is not null)
            return;

        try
        {
            var installed = await _provider.GetInstalledAsync();

            _installedVersions = installed
                .Where(p => !string.IsNullOrWhiteSpace(p.Id))
                .ToDictionary(p => p.Id, p => p.Version, StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            _installedVersions = new Dictionary<string, string>();
        }

        try
        {
            var updates = await _provider.GetUpdatesAsync();

            _availableUpdates = updates
                .Where(p => !string.IsNullOrWhiteSpace(p.Id))
                .ToDictionary(p => p.Id, p => p.Available, StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            _availableUpdates = new Dictionary<string, string>();
        }
    }

    private void Enrich(PackageModel package)
    {
        if (_installedVersions is not null
            && _installedVersions.TryGetValue(package.Id, out var installedVersion))
            package.Version = installedVersion;

        if (_availableUpdates is not null
            && _availableUpdates.TryGetValue(package.Id, out var availableVersion))
            package.Available = availableVersion;
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
// kaneki
