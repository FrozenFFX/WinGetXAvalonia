// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

namespace WinGetXAvalonia.Models;

public sealed class MarketplaceTab
{
    public MarketplaceKind Kind { get; set; }

    public string Title { get; set; } = "";

    public string Icon { get; set; } = "";

    public string Description { get; set; } = "";
}
