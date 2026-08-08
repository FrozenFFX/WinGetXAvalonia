// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;

namespace WinGetXAvalonia.Models;

public partial class WorkspacePackage : ObservableObject
{
    [ObservableProperty]
    private string id = string.Empty;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string version = string.Empty;

    [ObservableProperty]
    private string source = string.Empty;

    [ObservableProperty]
    [JsonIgnore]
    [NotifyPropertyChangedFor(nameof(StateText), nameof(IsBusy), nameof(CanInstall))]
    private PackageInstallState installState = PackageInstallState.NotInstalled;

    public string DisplayName => string.IsNullOrWhiteSpace(Name) ? Id : Name;

    public string StateText => InstallState switch
    {
        PackageInstallState.Installed => "Installed",
        PackageInstallState.Installing => "Installing...",
        PackageInstallState.Updating => "Updating...",
        _ => "Not installed"
    };

    public bool IsBusy => InstallState is PackageInstallState.Installing or PackageInstallState.Updating;

    public bool CanInstall => !IsBusy && InstallState != PackageInstallState.Installed;
}
