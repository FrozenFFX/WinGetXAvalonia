// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using CommunityToolkit.Mvvm.ComponentModel;
using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.ViewModels;

public partial class StudioFilterOption : ObservableObject
{
    public StudioToolStatus? Mode { get; init; }

    public string Label { get; init; } = "";

    [ObservableProperty]
    private bool isActive;
}
