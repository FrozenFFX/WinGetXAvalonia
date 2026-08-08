// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinGetXAvalonia.Core.Domain.Package;
using WinGetXAvalonia.Core.Providers;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Core;
using WinGetXAvalonia.Services.Operations;

namespace WinGetXAvalonia.ViewModels;

public partial class PackageDetailsViewModel : ObservableObject
{
    private const string Fallback = "Not available";

    private readonly IPackageProvider _provider;
    private readonly IOperationService _operations;
    private readonly IClipboardService _clipboard;
    private readonly ILauncherService _launcher;

    private PackageDetails _details = new();
    private PackageHealth _health = PackageHealth.None;

    [ObservableProperty]
    private PackageModel? selectedPackage;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string name = "";

    [ObservableProperty]
    private string publisher = Fallback;

    [ObservableProperty]
    private string description = "";

    [ObservableProperty]
    private string installedVersion = Fallback;

    [ObservableProperty]
    private string latestVersion = Fallback;

    [ObservableProperty]
    private string homepage = "";

    [ObservableProperty]
    private string license = Fallback;

    [ObservableProperty]
    private string source = "";

    public string DescriptionDisplay { get; private set; } = "";

    public string PublisherDisplay { get; private set; } = Fallback;

    public string HomepageDisplay { get; private set; } = "";

    public string LicenseDisplay { get; private set; } = Fallback;

    public string ReleaseNotesDisplay { get; private set; } = Fallback;

    public bool HasHomepage { get; private set; }

    public int HealthScore => _health.Score;

    public string HealthScoreLabel => _health.ScoreLabel;

    public string SourceTrustLabel => _health.SourceTrustLabel;

    public string LatestVersionLabel => _health.LatestVersionLabel;

    public string UpdateFrequencyLabel => _health.UpdateFrequencyLabel;

    public string PublisherLabel => _health.PublisherLabel;

    public PackageDetailsViewModel(
        IPackageProvider provider,
        IOperationService operations,
        IClipboardService clipboard,
        ILauncherService launcher)
    {
        _provider = provider;
        _operations = operations;
        _clipboard = clipboard;
        _launcher = launcher;

        SelectedPackage = null;

        Clear();
    }

    public async Task Load(PackageModel? package)
    {
        if (SelectedPackage?.Id == package?.Id &&
            !string.IsNullOrWhiteSpace(Name))
            return;

        SelectedPackage = package;

        _health = package is null
            ? PackageHealth.None
            : PackageHealthCalculator.Calculate(package);

        OnHealthChanged();

        ResetDetails();

        if (package is null || string.IsNullOrWhiteSpace(package.Id))
        {
            IsLoading = false;
            return;
        }

        IsLoading = true;

        try
        {
            SetDetails(await _provider.GetDetailsAsync(package.Id));
        }
        catch
        {
            SetDetails(new PackageDetails());
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void SetDetails(PackageDetails details)
    {
        _details = details ?? new PackageDetails();

        RefreshDisplay();
    }

    public void Clear()
    {
        SelectedPackage = null;

        _details = new();

        _health = PackageHealth.None;

        OnHealthChanged();

        IsLoading = false;

        RefreshDisplay();
    }

    [RelayCommand]
    private async Task Install()
    {
        if (SelectedPackage is null || string.IsNullOrWhiteSpace(SelectedPackage.Id))
            return;

        await _operations.InstallAsync(SelectedPackage.Id);

        await Load(SelectedPackage);
    }

    [RelayCommand]
    private async Task Update()
    {
        if (SelectedPackage is null || string.IsNullOrWhiteSpace(SelectedPackage.Id))
            return;

        await _operations.UpdateAsync(SelectedPackage.Id);

        await Load(SelectedPackage);
    }

    [RelayCommand]
    private async Task Remove()
    {
        if (SelectedPackage is null || string.IsNullOrWhiteSpace(SelectedPackage.Id))
            return;

        await _operations.RemoveAsync(SelectedPackage.Id);

        Clear();
    }

    [RelayCommand]
    private async Task CopyId()
    {
        if (SelectedPackage is null || string.IsNullOrWhiteSpace(SelectedPackage.Id))
            return;

        await _clipboard.CopyTextAsync(SelectedPackage.Id);

        return;
    }

    [RelayCommand]
    private async Task CopyName()
    {
        if (SelectedPackage is null || string.IsNullOrWhiteSpace(SelectedPackage.Name))
            return;

        await _clipboard.CopyTextAsync(SelectedPackage.Name);

        return;
    }

    [RelayCommand]
    private async Task OpenHomepage()
    {
        if (!Uri.TryCreate(
                HomepageDisplay,
                UriKind.Absolute,
                out _))
        {
            return;
        }

        await _launcher.OpenUrlAsync(HomepageDisplay);
    }

    private void ResetDetails()
    {
        _details = new PackageDetails();

        RefreshDisplay();
    }

    private void RefreshDisplay()
    {
        Name = Pick(_details.Name, SelectedPackage?.Name);
        Publisher = Pick(_details.Publisher);
        Description = Pick(_details.Description, SelectedPackage?.Description);
        InstalledVersion = Pick(_details.Version, SelectedPackage?.Version);
        LatestVersion = Pick(_details.LatestVersion, SelectedPackage?.Available);
        Homepage = _details.Homepage;
        License = Pick(_details.License);
        Source = Pick(_details.Source, SelectedPackage?.Source);

        DescriptionDisplay = Pick(_details.Description, SelectedPackage?.Description);
        PublisherDisplay = Pick(_details.Publisher);
        HomepageDisplay = _details.Homepage;
        LicenseDisplay = Pick(_details.License);
        ReleaseNotesDisplay = Pick(_details.ReleaseNotes);
        HasHomepage = !string.IsNullOrWhiteSpace(_details.Homepage);

        OnPropertyChanged(nameof(DescriptionDisplay));
        OnPropertyChanged(nameof(PublisherDisplay));
        OnPropertyChanged(nameof(HomepageDisplay));
        OnPropertyChanged(nameof(LicenseDisplay));
        OnPropertyChanged(nameof(ReleaseNotesDisplay));
        OnPropertyChanged(nameof(HasHomepage));

        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(Publisher));
        OnPropertyChanged(nameof(Description));
        OnPropertyChanged(nameof(InstalledVersion));
        OnPropertyChanged(nameof(LatestVersion));
        OnPropertyChanged(nameof(Homepage));
        OnPropertyChanged(nameof(License));
        OnPropertyChanged(nameof(Source));
    }

    private static string Pick(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
                return value.Trim();
        }

        return Fallback;
    }

    private void OnHealthChanged()
    {
        OnPropertyChanged(nameof(HealthScore));
        OnPropertyChanged(nameof(HealthScoreLabel));
        OnPropertyChanged(nameof(SourceTrustLabel));
        OnPropertyChanged(nameof(LatestVersionLabel));
        OnPropertyChanged(nameof(UpdateFrequencyLabel));
        OnPropertyChanged(nameof(PublisherLabel));
    }
}
