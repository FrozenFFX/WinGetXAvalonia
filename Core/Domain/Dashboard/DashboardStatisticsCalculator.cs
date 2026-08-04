using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Core.Domain.Dashboard;

public static class DashboardStatisticsCalculator
{
    public static DashboardStatistics Calculate(
        IReadOnlyCollection<InstalledPackage> installed,
        IReadOnlyCollection<UpdatePackage> updates,
        IReadOnlyCollection<AppPackage> sources,
        int favoriteCount)
        => new(installed.Count, updates.Count, sources.Count, favoriteCount);

    public static DashboardStatistics Calculate(
        IReadOnlyCollection<string> installedLines,
        IReadOnlyCollection<string> updateLines,
        IReadOnlyCollection<string> sourceLines,
        int favoriteCount)
        => new(installedLines.Count, updateLines.Count, sourceLines.Count, favoriteCount);
}

public sealed record DashboardStatistics(
    int InstalledCount,
    int UpdateCount,
    int SourceCount,
    int FavoriteCount);
