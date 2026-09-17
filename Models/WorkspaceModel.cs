// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace WinGetXAvalonia.Models;

public partial class WorkspaceModel : ObservableObject
{
    [ObservableProperty]
    private Guid id = Guid.NewGuid();

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string description = string.Empty;

    [ObservableProperty]
    private string icon = "workspace";

    [ObservableProperty]
    private DateTime created = DateTime.Now;

    [ObservableProperty]
    private DateTime modified = DateTime.Now;

    [ObservableProperty]
    private DateTime? lastUsed;

    [ObservableProperty]
    private bool favorite;

    [ObservableProperty]
    private ObservableCollection<WorkspacePackage> packages = new();

    public int PackageCount => Packages.Count;

    public WorkspaceModel()
    {
        Packages.CollectionChanged += (_, _) => OnPropertyChanged(nameof(PackageCount));
    }
}
