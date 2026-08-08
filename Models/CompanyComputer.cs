// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace WinGetXAvalonia.Models;

public partial class CompanyComputer : ObservableObject
{
    [ObservableProperty]
    private Guid id = Guid.NewGuid();

    [ObservableProperty]
    private string name = "";

    [ObservableProperty]
    private string os = "";

    [ObservableProperty]
    private string cpu = "";

    [ObservableProperty]
    private string ram = "";

    [ObservableProperty]
    private bool online;

    [ObservableProperty]
    private DateTime lastSeen = DateTime.Now;

    [ObservableProperty]
    private int pendingUpdates;

    [ObservableProperty]
    private int criticalUpdates;

    [ObservableProperty]
    private int failedInstallations;

    [ObservableProperty]
    private ObservableCollection<string> installedApps = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsBusy))]
    private bool isProcessing;

    public bool IsBusy => IsProcessing;

    public string StateText => Online ? "Online" : "Offline";

    public string StateBrush => Online ? "SuccessBrush" : "DangerBrush";

    public bool HasCriticalUpdates => CriticalUpdates > 0;

    public string InstalledSummary => InstalledApps.Count switch
    {
        0 => "No apps tracked",
        1 => "1 app",
        var count => $"{count} apps"
    };
}
