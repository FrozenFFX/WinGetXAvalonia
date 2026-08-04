// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Services.Core;

public interface IFeaturedCatalog
{
    Task<IReadOnlyList<FeaturedItem>> GetAsync();
}
