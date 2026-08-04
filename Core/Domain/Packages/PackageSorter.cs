using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Core.Domain.Packages;

public static class PackageSorter
{
    public static IOrderedEnumerable<PackageModel> Sort(
        IEnumerable<PackageModel> packages,
        PackageSortMode mode)
        => mode switch
        {
            PackageSortMode.Name => packages.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase),
            PackageSortMode.Version => packages.OrderBy(p => p.Version, PackageVersionComparer.Instance),
            PackageSortMode.Source => packages.OrderBy(p => p.Source, StringComparer.OrdinalIgnoreCase),
            PackageSortMode.Available => packages.OrderBy(p => p.Available, PackageVersionComparer.Instance),
            _ => packages.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
        };
}
