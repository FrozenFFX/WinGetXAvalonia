// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace WinGetXAvalonia.Models;

public partial class SoftwareIdentity : ObservableObject
{
    [ObservableProperty]
    private Guid id = Guid.NewGuid();

    [ObservableProperty]
    private string name = "";

    [ObservableProperty]
    private string publisher = "";

    [ObservableProperty]
    private string version = "";

    [ObservableProperty]
    private DateTime installedOn = DateTime.Now;

    [ObservableProperty]
    private DateTime? lastUpdatedOn;

    [ObservableProperty]
    private int updateCount;

    [ObservableProperty]
    private string source = "winget";

    [ObservableProperty]
    private string? workspace;

    [ObservableProperty]
    private string? blueprint;

    [ObservableProperty]
    private string? automation;

    [ObservableProperty]
    private int trustScore = 50;

    [ObservableProperty]
    private int healthScore = 100;

    [ObservableProperty]
    private double communityScore = 4.0;

    [ObservableProperty]
    private int rollbackCount;

    [ObservableProperty]
    private bool repairSupported;

    [ObservableProperty]
    private DateTime? lastUsed;

    [ObservableProperty]
    private ObservableCollection<string> dependencies = new();

    public string HealthText => $"{HealthScore}%";

    public string HealthBrush => HealthScore switch
    {
        >= 80 => "SuccessBrush",
        >= 50 => "WarningBrush",
        _ => "DangerBrush"
    };

    public string TrustText => $"{TrustScore}";

    public string TrustBrush => TrustScore switch
    {
        >= 80 => "SuccessBrush",
        >= 50 => "WarningBrush",
        _ => "DangerBrush"
    };

    public string CommunityDisplay => CommunityScore.ToString("0.0");

    public string InstalledDateDisplay => InstalledOn.ToString("dd MMM yyyy");

    public string UpdatedDateDisplay => LastUpdatedOn?.ToString("dd MMM yyyy") ?? "Never";

    public string DependenciesSummary => Dependencies.Count switch
    {
        0 => "None",
        1 => "1 dependency",
        var count => $"{count} dependencies"
    };
}
