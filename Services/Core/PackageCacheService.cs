// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System;
using Microsoft.Extensions.Caching.Memory;
using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Services.Core;

public sealed class PackageCacheService
{
    private const int SizeLimit = 512;

    private readonly MemoryCache _cache = new(new MemoryCacheOptions
    {
        SizeLimit = SizeLimit
    });

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
        _cache.Set(id, details, new MemoryCacheEntryOptions
        {
            Size = 1,
            SlidingExpiration = TimeSpan.FromMinutes(15)
        });
    }

    public void Clear()
    {
        _cache.Clear();
    }
}
