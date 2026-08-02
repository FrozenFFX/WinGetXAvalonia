using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using WinGetXAvalonia.Services;

namespace WinGetXAvalonia.ViewModels;

public partial class BrowseViewModel : ViewModelBase
{
    private readonly WingetService _winget;

    [ObservableProperty]
    private string _searchText = "";

    public ObservableCollection<string> Results { get; } = new();

    public BrowseViewModel(WingetService winget)
    {
        _winget = winget;
    }

    [RelayCommand]
    public async Task Search()
    {
        Results.Clear();

        if (string.IsNullOrWhiteSpace(SearchText))
            return;

        foreach (var item in await _winget.Search(SearchText))
            Results.Add(item);
    }

    [RelayCommand]
    async Task Install(string id)
    {
        await _winget.Install(id);
    }
}
