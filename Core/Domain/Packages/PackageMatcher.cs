// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Core.Domain.Packages;

public static class PackageMatcher
{
    public static bool MatchesId(PackageModel package, string id)
        => package is not null
           && !string.IsNullOrWhiteSpace(id)
           && string.Equals(package.Id, id, StringComparison.OrdinalIgnoreCase);

    public static PackageModel? FindById(IEnumerable<PackageModel> packages, string id)
        => packages.FirstOrDefault(p => MatchesId(p, id));

    public static PackageModel? FindPreferred(IEnumerable<PackageModel> packages, string id)
        => FindById(packages, id) ?? packages.FirstOrDefault();
}
