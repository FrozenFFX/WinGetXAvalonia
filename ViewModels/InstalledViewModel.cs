// ==========================================================
// WinGetX - https://github.com/FrozenFFX
// ==========================================================
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using WinGetXAvalonia.Core.Providers;
using WinGetXAvalonia.Core.State;
using WinGetXAvalonia.Helpers;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Core;
using WinGetXAvalonia.Services.Operations;

namespace WinGetXAvalonia.ViewModels;

// kaneki
public partial class InstalledViewModel : AsyncViewModel
{
    private readonly IPackageProvider _provider;
    private readonly IOperationService _operations;
    private readonly ClipboardService _clipboard;

    private readonly List<PackageModel> _allPackages = new();
    private bool _initialized;

    public ObservableCollection<PackageModel> Packages { get; } = new();

    public int PackageCount => Packages.Count;

    [ObservableProperty]
    private string searchText = "";

    [ObservableProperty]
    private string? selectedSort;

    [ObservableProperty]
    private string? selectedFilter;

    public InstalledViewModel(IPackageProvider provider, IOperationService operations, ClipboardService clipboard)
    {
        _provider = provider;
        _operations = operations;
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
        State = LoadState.Loading;
        ErrorMessage = null;

        try
        {
            var apps = await _provider.GetInstalledAsync();

            _allPackages.Clear();

            foreach (var app in apps)
                _allPackages.Add(ToModel(app));

            ApplyFilter();

            State = _allPackages.Count == 0 ? LoadState.Empty : LoadState.Loaded;
        }
        catch (Exception ex)
        {
            State = LoadState.Error;
            ErrorMessage = ex.Message;
        }
    }

    [RelayCommand]
    private async Task Update(PackageModel package)
    {
        if (string.IsNullOrWhiteSpace(package.Id))
            return;

        await _operations.UpdateAsync(package.Id);
    }

    [RelayCommand]
    private async Task Remove(PackageModel package)
    {
        if (package is null)
            return;

        Packages.Remove(package);
        _allPackages.Remove(package);

        await _operations.RemoveAsync(package.Id);
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

        OnPropertyChanged(nameof(PackageCount));
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
// kaneki
