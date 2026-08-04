// ==========================================================
// WinGetX - https://github.com/FrozenFFX
// ==========================================================
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using WinGetXAvalonia.Core.Providers;
using WinGetXAvalonia.Core.State;
using WinGetXAvalonia.Helpers;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Operations;

namespace WinGetXAvalonia.ViewModels;

// kaneki
public partial class InstalledViewModel : AsyncViewModel
{
    private readonly IPackageProvider _provider;
    private readonly IOperationService _operations;

    private readonly List<PackageModel> _allPackages = new();
    private bool _initialized;

    public event EventHandler? BrowseRequested;

    public ObservableCollection<PackageModel> Packages { get; } = new();

    public int PackageCount => Packages.Count;

    public int SelectedCount => Packages.Count(x => x.Selected);

    [ObservableProperty]
    private string searchText = "";

    [ObservableProperty]
    private string? selectedSort;

    [ObservableProperty]
    private string? selectedFilter;

    public InstalledViewModel(IPackageProvider provider, IOperationService operations)
    {
        _provider = provider;
        _operations = operations;
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
        Unsubscribe(package);

        await _operations.RemoveAsync(package.Id);

        OnPropertyChanged(nameof(PackageCount));
        OnPropertyChanged(nameof(SelectedCount));
    }

    [RelayCommand]
    private void CopyId(PackageModel package)
    {
        if (package is null)
            return;

        _ = CurrentWindow?.Clipboard?.SetTextAsync(package.Id);
    }

    [RelayCommand]
    private void CopyName(PackageModel package)
    {
        if (package is null)
            return;

        _ = CurrentWindow?.Clipboard?.SetTextAsync(package.Name);
    }

    [RelayCommand]
    private async Task OpenHomepage(PackageModel package)
    {
        if (package is null)
            return;

        var url = await _provider.GetHomepageAsync(package.Id);

        if (!string.IsNullOrWhiteSpace(url))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
    }

    [RelayCommand]
    private void Browse()
        => BrowseRequested?.Invoke(this, EventArgs.Empty);

    [RelayCommand]
    private async Task RemoveSelected()
    {
        var selected = Packages
            .Where(x => x.Selected)
            .ToList();

        foreach (var package in selected)
        {
            await _operations.RemoveAsync(package.Id);

            Packages.Remove(package);
            _allPackages.Remove(package);
            Unsubscribe(package);
        }

        OnPropertyChanged(nameof(PackageCount));
        OnPropertyChanged(nameof(SelectedCount));
    }

    [RelayCommand]
    private async Task UpdateSelected()
    {
        var selected = Packages
            .Where(x => x.Selected)
            .ToList();

        foreach (var package in selected)
            await _operations.UpdateAsync(package.Id);

        await Refresh();
    }

    private void ApplyFilter()
    {
        UnsubscribeAll();

        Packages.Clear();

        IEnumerable<PackageModel> filtered = _allPackages;

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            filtered = filtered.Where(p =>
                p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                || p.Id.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
        }

        foreach (var package in filtered)
        {
            Subscribe(package);
            Packages.Add(package);
        }

        OnPropertyChanged(nameof(PackageCount));
        OnPropertyChanged(nameof(SelectedCount));
    }

    private void Subscribe(PackageModel package)
        => package.PropertyChanged += OnPackagePropertyChanged;

    private void Unsubscribe(PackageModel package)
        => package.PropertyChanged -= OnPackagePropertyChanged;

    private void UnsubscribeAll()
    {
        foreach (var package in Packages)
            Unsubscribe(package);
    }

    private void OnPackagePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PackageModel.Selected))
            OnPropertyChanged(nameof(SelectedCount));
    }

    private static Avalonia.Controls.Window? CurrentWindow
        => Application.Current?.ApplicationLifetime
            is IClassicDesktopStyleApplicationLifetime { MainWindow: { } window }
            ? window
            : null;

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
