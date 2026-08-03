using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using WinGetXAvalonia.Core.Providers;
using WinGetXAvalonia.Helpers;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services;

namespace WinGetXAvalonia.ViewModels;

public partial class InstalledViewModel : ViewModelBase
{
    private readonly IPackageProvider _provider;
    private readonly ClipboardService _clipboard;

    private readonly List<PackageModel> _allPackages = new();
    private bool _initialized;

    public ObservableCollection<PackageModel> Packages { get; } = new();

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string searchText = "";

    public InstalledViewModel(IPackageProvider provider, ClipboardService clipboard)
    {
        _provider = provider;
        _clipboard = clipboard;
    }

    public async Task InitializeAsync()
    {
        if (_initialized)
            return;

        _initialized = true;
        await Refresh();
    }

    partial void OnSearchTextChanged(string value)
        => ApplyFilter();

    [RelayCommand]
    public async Task Refresh()
    {
        try
        {
            IsLoading = true;

            var apps = await _provider.GetInstalledAsync();

            _allPackages.Clear();

            foreach (var app in apps)
                _allPackages.Add(ToModel(app));

            ApplyFilter();
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task Update(PackageModel package)
    {
        if (string.IsNullOrWhiteSpace(package.Id))
            return;

        await _provider.UpgradeAsync(package.Id);
    }

    [RelayCommand]
    private async Task Remove(PackageModel package)
    {
        if (package is null)
            return;

        Packages.Remove(package);
        _allPackages.Remove(package);

        try
        {
            await _provider.UninstallAsync(package.Id);
        }
        catch
        {
            _allPackages.Add(package);
            ApplyFilter();
        }
    }

    [RelayCommand]
    private Task CopyId(PackageModel package)
        => _clipboard.CopyTextAsync(package.Id);

    [RelayCommand]
    private Task CopyName(PackageModel package)
        => _clipboard.CopyTextAsync(package.Name);

    [RelayCommand]
    private async Task OpenHomepage(PackageModel package)
    {
        if (string.IsNullOrWhiteSpace(package.Id))
            return;

        var url = await _provider.GetHomepageAsync(package.Id);

        if (!string.IsNullOrWhiteSpace(url))
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }

    private void ApplyFilter()
    {
        Packages.Clear();

        IEnumerable<PackageModel> filtered = _allPackages;

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            filtered = filtered.Where(p =>
                p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                || p.Id.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
        }

        foreach (var package in filtered)
            Packages.Add(package);
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
