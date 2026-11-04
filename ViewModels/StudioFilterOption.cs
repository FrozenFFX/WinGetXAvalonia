// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using CommunityToolkit.Mvvm.ComponentModel;
using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.ViewModels;

public partial class StudioFilterOption : ObservableObject
{
    public string Key { get; init; } = "";

    public StudioToolStatus? Mode { get; init; }

    [ObservableProperty]
    private string label = "";

    public bool FavoritesOnly { get; init; }

    [ObservableProperty]
    private bool isActive;
}
