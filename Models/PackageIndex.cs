namespace WinGetXAvalonia.Models;

public sealed class PackageIndex
{
    private readonly Dictionary<string, PackageIndexEntry> _entries = new(StringComparer.OrdinalIgnoreCase);

    public void AddInstalled(IReadOnlyList<InstalledPackage> installed)
    {
        foreach (var package in installed)
        {
            if (string.IsNullOrWhiteSpace(package.Id))
                continue;

            if (!_entries.TryGetValue(package.Id, out var entry))
                entry = _entries[package.Id] = new PackageIndexEntry();

            entry.InstalledVersion = package.Version;
            entry.Source = package.Source;
        }
    }

    public void AddUpdates(IReadOnlyList<UpdatePackage> updates)
    {
        foreach (var package in updates)
        {
            if (string.IsNullOrWhiteSpace(package.Id))
                continue;

            if (!_entries.TryGetValue(package.Id, out var entry))
                entry = _entries[package.Id] = new PackageIndexEntry();

            entry.AvailableVersion = package.Available;
        }
    }

    public bool TryGet(string id, out PackageIndexEntry? entry)
        => _entries.TryGetValue(id, out entry);
}

public sealed class PackageIndexEntry
{
    public string InstalledVersion { get; set; } = "";

    public string AvailableVersion { get; set; } = "";

    public string Source { get; set; } = "";
}
