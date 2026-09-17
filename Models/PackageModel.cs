// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using CommunityToolkit.Mvvm.ComponentModel;
using WinGetXAvalonia.Core.Domain.Packages;

namespace WinGetXAvalonia.Models;

public partial class PackageModel : ObservableObject
{
    [ObservableProperty]
    private string icon = "package";

    [ObservableProperty]
    private string name = "";

    [ObservableProperty]
    private string id = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(InstalledVersionDisplay))]
    private string version = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasUpdate), nameof(InstallButtonText), nameof(CanInstall), nameof(AvailableVersionDisplay))]
    private string available = "";

    [ObservableProperty]
    private string source = "";

    [ObservableProperty]
    private string description = "";

    [ObservableProperty]
    private bool isFeatured;

    [ObservableProperty]
    private bool selected;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsBusy), nameof(InstallButtonText), nameof(CanInstall), nameof(InstalledVersionDisplay))]
    private PackageInstallState installState = PackageInstallState.NotInstalled;

    [ObservableProperty]
    private int score;

    public bool HasUpdate => !string.IsNullOrWhiteSpace(Available);

    public bool IsBusy => InstallState is PackageInstallState.Installing or PackageInstallState.Updating;

    public string InstallButtonText =>
        PackageFormatter.InstallButtonText(InstallState, HasUpdate);

    public bool CanInstall => !IsBusy
        && (InstallState == PackageInstallState.NotInstalled
            || (InstallState == PackageInstallState.Installed && HasUpdate));

    public string InstalledVersionDisplay =>
        PackageFormatter.ValueOrDash(
            InstallState == PackageInstallState.Installed ? Version : null);

    public string AvailableVersionDisplay =>
        PackageFormatter.ValueOrDash(Available);
}
