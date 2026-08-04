// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Core.Domain.Packages;

public static class PackageFilter
{
    public static bool IsMatch(PackageModel package, string? searchText, PackageFilterMode mode)
    {
        if (package is null)
            return false;

        if (!string.IsNullOrWhiteSpace(searchText)
            && !package.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase)
            && !package.Id.Contains(searchText, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

        return mode switch
        {
            PackageFilterMode.Winget => package.Source.Contains("winget", StringComparison.OrdinalIgnoreCase),
            PackageFilterMode.MsStore => package.Source.Contains("store", StringComparison.OrdinalIgnoreCase),
            PackageFilterMode.Portable => package.Source.Contains("portable", StringComparison.OrdinalIgnoreCase),
            _ => true
        };
    }

    public static IEnumerable<PackageModel> Apply(
        IEnumerable<PackageModel> packages,
        string? searchText,
        PackageFilterMode mode)
        => packages.Where(p => IsMatch(p, searchText, mode));
}
