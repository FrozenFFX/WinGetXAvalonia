// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using Avalonia.Platform;
using WinGetXAvalonia.Helpers;
using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Services.Core;

public sealed class FeaturedCatalog : IFeaturedCatalog
{
    private const string AssetUri = "avares://WinGetXAvalonia/Assets/featured.json";

    private IReadOnlyList<FeaturedItem>? _cache;

    public async Task<IReadOnlyList<FeaturedItem>> GetAsync()
    {
        if (_cache is not null)
            return _cache;

        using Stream stream = AssetLoader.Open(new Uri(AssetUri), null);
        using var reader = new StreamReader(stream);

        _cache = FeaturedParser.Parse(await reader.ReadToEndAsync());

        return _cache;
    }
}
