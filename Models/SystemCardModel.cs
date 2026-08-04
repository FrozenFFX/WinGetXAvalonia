// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using CommunityToolkit.Mvvm.ComponentModel;

namespace WinGetXAvalonia.Models;

public partial class SystemCardModel : ObservableObject
{
    [ObservableProperty]
    private string title = "";

    [ObservableProperty]
    private string value = "";

    [ObservableProperty]
    private string icon = "";
}
