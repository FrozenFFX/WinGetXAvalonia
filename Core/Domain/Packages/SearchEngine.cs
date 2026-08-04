using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Core.Domain.Packages;

public enum SearchMatchMode
{
    Contains,
    StartsWith,
    Exact
}

public sealed record SearchOptions(
    SearchMatchMode MatchMode = SearchMatchMode.Contains,
    bool InstalledFirst = false,
    bool UpdateAvailableFirst = false,
    bool FeaturedBoost = false)
{
    public static readonly SearchOptions Default = new();
}

public static class SearchEngine
{
    public static IReadOnlyList<PackageModel> Rank(
        IEnumerable<PackageModel> results,
        string query,
        SearchOptions? options = null)
    {
        options ??= SearchOptions.Default;

        if (string.IsNullOrWhiteSpace(query))
            return results.ToList();

        return results
            .Select(p => (Package: p, Score: Score(p, query, options.MatchMode)))
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => options.FeaturedBoost && x.Package.IsFeatured)
            .ThenByDescending(x => options.InstalledFirst && IsInstalled(x.Package))
            .ThenByDescending(x => options.UpdateAvailableFirst && x.Package.HasUpdate)
            .ThenBy(x => x.Package.Name, StringComparer.OrdinalIgnoreCase)
            .Select(x => x.Package)
            .ToList();
    }

    public static int Score(
        PackageModel package,
        string query,
        SearchMatchMode mode = SearchMatchMode.Contains)
    {
        if (package is null || string.IsNullOrWhiteSpace(query))
            return 0;

        if (string.Equals(package.Name, query, StringComparison.OrdinalIgnoreCase))
            return 100;

        if (string.Equals(package.Id, query, StringComparison.OrdinalIgnoreCase))
            return 95;

        if (mode == SearchMatchMode.Exact)
            return 0;

        if (package.Name.StartsWith(query, StringComparison.OrdinalIgnoreCase))
            return 85;

        if (package.Id.StartsWith(query, StringComparison.OrdinalIgnoreCase))
            return 75;

        if (mode == SearchMatchMode.StartsWith)
            return 0;

        if (package.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
            return 50;

        if (package.Id.Contains(query, StringComparison.OrdinalIgnoreCase))
            return 40;

        return 0;
    }

    private static bool IsInstalled(PackageModel package)
        => package.InstallState == PackageInstallState.Installed;
}
