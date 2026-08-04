using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinGetXAvalonia.Core.State;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Studio;

namespace WinGetXAvalonia.ViewModels;

public partial class StudioViewModel : AsyncViewModel
{
    private readonly IStudioService _studio;

    private readonly List<StudioToolViewModel> _allTools = new();

    public ObservableCollection<StudioToolViewModel> Tools { get; } = new();

    public ObservableCollection<StudioFilterOption> Filters { get; } = new();

    [ObservableProperty]
    private string searchText = "";

    [ObservableProperty]
    private StudioToolStatus? selectedFilter;

    [ObservableProperty]
    private StudioToolViewModel? selectedTool;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ToolsSummary))]
    private int toolCount;

    public string ToolsSummary => ToolCount switch
    {
        0 => "No tools",
        1 => "1 tool",
        var count => $"{count} tools"
    };

    public StudioViewModel(IStudioService studio)
    {
        _studio = studio;

        SeedFilters();
    }

    public Task InitializeAsync()
        => RefreshAsync();

    partial void OnSearchTextChanged(string value)
        => ApplyFilter();

    partial void OnSelectedFilterChanged(StudioToolStatus? value)
    {
        foreach (var filter in Filters)
            filter.IsActive = filter.Mode == value;

        ApplyFilter();
    }

    [RelayCommand]
    private void SelectFilter(StudioFilterOption? option)
    {
        if (option is not null)
            SelectedFilter = option.Mode;
    }

    [RelayCommand]
    private void SelectTool(StudioToolViewModel? tool)
    {
        if (tool is not null)
            SelectedTool = tool;
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        State = LoadState.Loading;
        ErrorMessage = null;

        try
        {
            var tools = await _studio.GetToolsAsync();

            _allTools.Clear();

            foreach (var tool in tools.OrderBy(t => t.Title))
                _allTools.Add(new StudioToolViewModel(tool));

            ToolCount = _allTools.Count;

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

        if (SelectedFilter is { } mode)
            filtered = filtered.Where(t => t.Status == mode);

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
    }

    private void SeedFilters()
    {
        Filters.Add(new() { Mode = null, Label = "All", IsActive = true });
        Filters.Add(new() { Mode = StudioToolStatus.Ready, Label = "Ready" });
        Filters.Add(new() { Mode = StudioToolStatus.Experimental, Label = "Experimental" });
        Filters.Add(new() { Mode = StudioToolStatus.ComingSoon, Label = "Coming Soon" });
    }
}
