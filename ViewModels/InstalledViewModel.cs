using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using WinGetXAvalonia.Core.Providers;
using WinGetXAvalonia.Helpers;
using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.ViewModels;

public partial class InstalledViewModel : ViewModelBase
{
    private readonly IPackageProvider _provider;

    public ObservableCollection<PackageModel> Packages { get; } = new();

    [ObservableProperty]
    private bool isLoading;

    public InstalledViewModel(IPackageProvider provider)
    {
        _provider = provider;
        _ = Refresh();
    }

    [RelayCommand]
    public async Task Refresh()
    {
        IsLoading = true;

        Packages.Clear();

        var apps = await _provider.GetInstalledAsync();

        foreach (var app in apps)
            Packages.Add(ToModel(app));

        IsLoading = false;
    }

    [RelayCommand]
    async Task Remove(PackageModel package)
    {
        await _provider.UninstallAsync(package.Id);

        await Refresh();
    }

    private static PackageModel ToModel(InstalledPackage app)
        => new()
        {
            Icon = IconProvider.Get(app.Name),
            Name = app.Name,
            Id = app.Id,
            Version = app.Version,
            Source = app.Source
        };
}
