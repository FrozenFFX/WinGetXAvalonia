// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Collections.ObjectModel;

namespace WinGetXAvalonia.Models;

public class NavigationCategory
{
    public string Title { get; set; } = "";

    public string Icon { get; set; } = "";

    public ObservableCollection<NavigationItem> Items { get; } = new();

    public bool IsExpanded { get; set; } = true;
}
