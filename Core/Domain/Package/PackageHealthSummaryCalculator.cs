using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Core.Domain.Package;

public sealed record PackageHealthSummary(
    int Total,
    int Healthy,
    int Outdated,
    int Deprecated,
    int UnknownSource,
    int Failed)
{
    public static readonly PackageHealthSummary Empty = new(0, 0, 0, 0, 0, 0);
}

public static class PackageHealthSummaryCalculator
{
    private const int HealthyThreshold = 80;

    public static PackageHealthSummary Calculate(
        IEnumerable<InstalledPackage> installed,
        IEnumerable<UpdatePackage> updates,
        IEnumerable<HistoryEntry> history)
    {
        ArgumentNullException.ThrowIfNull(installed);
        ArgumentNullException.ThrowIfNull(updates);
        ArgumentNullException.ThrowIfNull(history);

        var installedList = installed.ToList();
        var availableBySource = BuildAvailableIndex(updates);

        int healthy = 0;
        int outdated = 0;
        int deprecated = 0;
        int unknownSource = 0;

        foreach (InstalledPackage package in installedList)
        {
            var available = GetAvailable(package, availableBySource);

            PackageHealth health = PackageHealthCalculator.Calculate(
                package.Source, package.Version, available);

            if (health.SourceTrust == SourceTrustLevel.Unknown)
                unknownSource++;

            if (health.UpdateStatus == UpdateStatus.UpdateAvailable)
                outdated++;
            else if (health.Score >= HealthyThreshold)
                healthy++;
            else
                deprecated++;
        }

        int failed = history.Count(entry => !entry.Success);

        return new PackageHealthSummary(
            Total: installedList.Count,
            Healthy: healthy,
            Outdated: outdated,
            Deprecated: deprecated,
            UnknownSource: unknownSource,
            Failed: failed);
    }

    private static Dictionary<string, string> BuildAvailableIndex(IEnumerable<UpdatePackage> updates)
    {
        var index = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (UpdatePackage update in updates)
        {
            if (string.IsNullOrWhiteSpace(update.Id))
                continue;

            index.TryAdd(update.Id, update.Available);
        }

        return index;
    }

    private static string GetAvailable(InstalledPackage package, Dictionary<string, string> index)
    {
        if (string.IsNullOrWhiteSpace(package.Id))
            return "";

        index.TryGetValue(package.Id, out string? available);
        return available ?? "";
    }
}
