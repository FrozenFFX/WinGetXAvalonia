// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

namespace WinGetXAvalonia.Models;

public class NavigationItem
{
    public string Title { get; set; } = "";

    public string Icon { get; set; } = "";

    public Type Page { get; set; } = typeof(object);
}
