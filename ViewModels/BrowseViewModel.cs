using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using WinGetXAvalonia.Core.Providers;
using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.ViewModels;

public partial class BrowseViewModel : ViewModelBase
{
    private readonly IPackageProvider _provider;

    [ObservableProperty]
    private string searchText = "";

    public ObservableCollection<PackageModel> Results { get; } = new();

    public BrowseViewModel(IPackageProvider provider)
    {
        _provider = provider;
    }

    [RelayCommand]
    public async Task Search()
    {
        Results.Clear();

        if (string.IsNullOrWhiteSpace(SearchText))
            return;

        foreach (var item in await _provider.SearchAsync(SearchText))
            Results.Add(item);
    }

    [RelayCommand]
    async Task Install(PackageModel package)
    {
        await _provider.InstallAsync(package.Id);
    }
}
