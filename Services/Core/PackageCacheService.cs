// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Collections.Generic;
using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Services.Core;

public sealed class PackageCacheService
{
    private readonly Dictionary<string, WinGetXAvalonia.Models.PackageDetails> _cache = new();

    public bool TryGet(
        string id,
        out WinGetXAvalonia.Models.PackageDetails details)
    {
        return _cache.TryGetValue(id, out details!);
    }

    public void Store(
        string id,
        WinGetXAvalonia.Models.PackageDetails details)
    {
        _cache[id] = details;
    }

    public void Clear()
    {
        _cache.Clear();
    }
}
