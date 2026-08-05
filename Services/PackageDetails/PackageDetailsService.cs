// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using WinGetXAvalonia.Core.Providers;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Core;

namespace WinGetXAvalonia.Services.PackageDetails;

public class PackageDetailsService : IPackageDetailsService
{
    private readonly IPackageProvider _provider;
    private readonly PackageCacheService _cache;

    public PackageDetailsService(
        IPackageProvider provider,
        PackageCacheService cache)
    {
        _provider = provider;
        _cache = cache;
    }

    public async Task<PackageDetailsModel?> GetAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return null;

        if (_cache.TryGet(id, out var cached))
            return ToModel(cached);

        var details = await _provider.GetDetailsAsync(id).ConfigureAwait(false);

        _cache.Store(id, details);

        return ToModel(details);
    }

    private static PackageDetailsModel ToModel(WinGetXAvalonia.Models.PackageDetails details)
        => new()
        {
            Name = details.Name,
            Id = details.Id,
            Publisher = details.Publisher,
            Homepage = details.Homepage,
            License = details.License,
            InstalledVersion = details.Version,
            LatestVersion = details.LatestVersion,
            Source = details.Source,
            Description = details.Description
        };
}
