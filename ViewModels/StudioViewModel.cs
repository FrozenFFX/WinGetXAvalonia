// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinGetXAvalonia.Core.State;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Backup;
using WinGetXAvalonia.Services.Interfaces;
using WinGetXAvalonia.Services.Localization;
using WinGetXAvalonia.Services.Studio;

namespace WinGetXAvalonia.ViewModels;

public partial class StudioViewModel : AsyncViewModel
{
    private readonly IStudioService _studio;

    private readonly ISettingsService? _settings;

    private readonly AutoSaveService? _autoSave;

    private readonly LocalizationManager? _localization;

    private readonly List<StudioToolViewModel> _allTools = new();

    public ObservableCollection<StudioToolViewModel> Tools { get; } = new();

    public ObservableCollection<StudioFilterOption> Filters { get; } = new();

    public ObservableCollection<StudioFilterOption> Categories { get; } = new();

    public event EventHandler<string>? OpenToolRequested;

    [ObservableProperty]
    private string searchText = "";

    [ObservableProperty]
    private StudioToolStatus? selectedFilter;

    [ObservableProperty]
    private string selectedCategory = "All categories";

    [ObservableProperty]
    private bool isFavoritesOnly;

    [ObservableProperty]
    private StudioToolViewModel? selectedTool;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ToolsSummary))]
    private int toolCount;

    public string ToolsSummary => ToolCount switch
    {
        0 => Translate("No tools"),
        1 => Translate("1 tool"),
        var count => Translate($"{count} tools")
    };

    public bool IsToolSelected => SelectedTool is not null;

    public bool IsFilteredEmpty => IsLoaded && _allTools.Count > 0 && Tools.Count == 0;

    partial void OnSelectedToolChanged(StudioToolViewModel? value)
        => OnPropertyChanged(nameof(IsToolSelected));

    public StudioViewModel(
        IStudioService studio,
        ISettingsService? settings = null,
        AutoSaveService? autoSave = null,
        LocalizationManager? localization = null)
    {
        _studio = studio;
        _settings = settings;
        _autoSave = autoSave;
        _localization = localization;

        SeedFilters();
        SeedCategories([]);

        if (_localization is not null)
            _localization.LanguageChanged += OnLanguageChanged;
    }

    public override Task InitializeAsync()
        => RefreshAsync();

    partial void OnSearchTextChanged(string value)
        => ApplyFilter();

    partial void OnSelectedFilterChanged(StudioToolStatus? value)
    {
        foreach (var filter in Filters)
            filter.IsActive =
                !filter.FavoritesOnly
                && !IsFavoritesOnly
                && filter.Mode == value;

        ApplyFilter();
    }

    partial void OnSelectedCategoryChanged(string value)
        => ApplyFilter();

    partial void OnIsFavoritesOnlyChanged(bool value)
        => ApplyFilter();

    [RelayCommand]
    private void SelectFilter(StudioFilterOption? option)
    {
        if (option is null)
            return;

        IsFavoritesOnly = option.FavoritesOnly;
        SelectedFilter = option.Mode;

        foreach (var filter in Filters)
            filter.IsActive = filter.Key == option.Key;

        ApplyFilter();
    }

    [RelayCommand]
    private void SelectCategory(StudioFilterOption? option)
    {
        if (option is not null)
            SelectedCategory = option.Key;
    }

    [RelayCommand]
    private void SelectTool(StudioToolViewModel? tool)
    {
        if (tool is not null)
            SelectedTool = tool;
    }

    [RelayCommand]
    private void OpenTool(StudioToolViewModel? tool)
    {
        if (tool is { CanOpen: true })
            OpenToolRequested?.Invoke(this, tool.Tool.Id);
    }

    [RelayCommand]
    private void ToggleFavorite(StudioToolViewModel? tool)
    {
        if (tool is null)
            return;

        tool.IsFavorite = !tool.IsFavorite;
        var favoriteIds = _allTools
            .Where(item => item.IsFavorite)
            .Select(item => item.Tool.Id)
            .ToList();

        _autoSave?.Schedule(settings => settings.FavoriteStudioTools = favoriteIds);
        ApplyFilter();
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        State = LoadState.Loading;
        ErrorMessage = null;

        try
        {
            var tools = await _studio.GetToolsAsync();
            var favoriteIds = _settings?.Settings.FavoriteStudioTools
                ?? [];

            _allTools.Clear();

            foreach (var tool in tools.OrderBy(t => t.Title))
                _allTools.Add(new StudioToolViewModel(
                    tool,
                    _localization,
                    favoriteIds.Contains(tool.Id, StringComparer.OrdinalIgnoreCase)));

            SeedCategories(_allTools.Select(tool => tool.Tool.Category));

            ApplyFilter();

            State = _allTools.Count == 0 ? LoadState.Empty : LoadState.Loaded;
        }
        catch (Exception ex)
        {
            State = LoadState.Error;
            ErrorMessage = ex.Message;
        }
    }

    private void ApplyFilter()
    {
        IEnumerable<StudioToolViewModel> filtered = _allTools;

        if (IsFavoritesOnly)
            filtered = filtered.Where(tool => tool.IsFavorite);

        if (SelectedFilter is { } mode)
            filtered = filtered.Where(t => t.Status == mode);

        if (!string.IsNullOrEmpty(SelectedCategory)
            && SelectedCategory != "All categories")
        {
            filtered = filtered.Where(tool =>
                string.Equals(
                    tool.Tool.Category,
                    SelectedCategory,
                    StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            filtered = filtered.Where(t =>
                t.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                || t.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                || t.Category.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
        }

        Tools.Clear();

        foreach (var tool in filtered)
            Tools.Add(tool);

        ToolCount = Tools.Count;

        OnPropertyChanged(nameof(IsFilteredEmpty));
    }

    private void SeedFilters()
    {
        Filters.Add(new() { Key = "All", Mode = null, Label = Translate("All"), IsActive = true });
        Filters.Add(new() { Key = "Ready", Mode = StudioToolStatus.Ready, Label = Translate("Ready") });
        Filters.Add(new() { Key = "Experimental", Mode = StudioToolStatus.Experimental, Label = Translate("Experimental") });
        Filters.Add(new() { Key = "Coming Soon", Mode = StudioToolStatus.ComingSoon, Label = Translate("Coming Soon") });
        Filters.Add(new() { Key = "Favorites", FavoritesOnly = true, Label = Translate("Favorites") });
    }

    private void SeedCategories(IEnumerable<string> categories)
    {
        var selectedCategoryExists = Categories.Any(item => item.Key == SelectedCategory);
        Categories.Clear();
        Categories.Add(new()
        {
            Key = "All categories",
            Label = Translate("All categories")
        });

        foreach (var category in categories
                     .Where(value => !string.IsNullOrWhiteSpace(value))
                     .Distinct(StringComparer.OrdinalIgnoreCase)
                     .OrderBy(value => value, StringComparer.OrdinalIgnoreCase))
        {
            Categories.Add(new()
            {
                Key = category,
                Label = Translate(category)
            });
        }

        if (!selectedCategoryExists)
            SelectedCategory = "All categories";
    }

    private string Translate(string value)
        => _localization?.TranslateValue(value) ?? value;

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        foreach (var filter in Filters)
            filter.Label = Translate(filter.Key);

        foreach (var category in Categories)
            category.Label = Translate(category.Key);

        OnPropertyChanged(nameof(ToolsSummary));
    }
}
