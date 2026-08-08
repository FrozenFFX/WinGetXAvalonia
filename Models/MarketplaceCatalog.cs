// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

namespace WinGetXAvalonia.Models;

public sealed class MarketplaceCatalog
{
    public int Version { get; set; } = 1;

    public List<MarketplaceItem> Items { get; set; } = new();
}
