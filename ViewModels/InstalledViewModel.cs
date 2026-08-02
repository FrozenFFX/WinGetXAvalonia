using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using WinGetXAvalonia.Helpers;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services;

namespace WinGetXAvalonia.ViewModels;

public partial class InstalledViewModel : ViewModelBase
{
    private readonly WingetService _winget;

    public ObservableCollection<PackageModel> Packages { get; } = new();

    [ObservableProperty]
    private bool isLoading;

    public InstalledViewModel(WingetService winget)
    {
        _winget = winget;
        _ = Refresh();
    }

    [RelayCommand]
    public async Task Refresh()
    {
        IsLoading = true;

        Packages.Clear();

        var list = WingetParser.Parse(await _winget.GetInstalledApps());

        foreach (var app in list)
            Packages.Add(app);

        IsLoading = false;
    }

    [RelayCommand]
    async Task Remove(PackageModel package)
    {
        await _winget.Uninstall(package);

        await Refresh();
    }
}
