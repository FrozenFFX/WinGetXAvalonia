// ==========================================================
// WinGetX - https://github.com/FrozenFFX
// ==========================================================
using System.Text.RegularExpressions;
using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Helpers;

// kaneki
public static class WingetParser
{
    public static List<InstalledPackage> ParseInstalled(List<string> lines)
    {
        var list = new List<InstalledPackage>();

        foreach (string line in lines.Skip(2))
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var row = SplitRow(line);
            if (row.Id is null)
                continue;

            list.Add(new InstalledPackage
            {
                Name = row.Name,
                Id = row.Id,
                Version = row.Version,
                Source = row.Source
            });
        }

        return list;
    }

    public static List<UpdatePackage> ParseUpdates(List<string> lines)
    {
        var list = new List<UpdatePackage>();

        foreach (string line in lines.Skip(2))
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var row = SplitRow(line);
            if (row.Id is null)
                continue;

            list.Add(new UpdatePackage
            {
                Name = row.Name,
                Id = row.Id,
                Version = row.Version,
                Available = row.Available,
                Source = row.Source
            });
        }

        return list;
    }

    public static List<PackageModel> ParseSearch(List<string> lines)
    {
        var list = new List<PackageModel>();

        foreach (string line in lines.Skip(2))
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var row = SplitRow(line);
            if (row.Id is null)
                continue;

            list.Add(new PackageModel
            {
                Name = row.Name,
                Id = row.Id,
                Version = row.Version,
                Source = row.Source,
                Icon = IconProvider.Get(row.Name)
            });
        }

        return list;
    }

    public static string? ParseHomepage(IEnumerable<string> lines)
    {
        var match = Regex.Match(string.Join('\n', lines), @"(?im)^\s*Homepage:\s*(\S+)");

        if (match.Success && Uri.TryCreate(match.Groups[1].Value, UriKind.Absolute, out _))
            return match.Groups[1].Value;

        return null;
    }

    private static (string Name, string? Id, string Version, string Available, string Source) SplitRow(string line)
    {
        string[] p = Regex.Split(line.Trim(), @"\s{2,}");

        if (p.Length < 3)
            return default;

        int sourceIndex = -1;

        for (int i = p.Length - 1; i > 1; i--)
        {
            if (p[i] is "winget" or "msstore")
            {
                sourceIndex = i;
                break;
            }
        }

        int versionEnd = sourceIndex > 0 ? sourceIndex : p.Length;

        return (
            p[0],
            p[1],
            CleanVersion(p.ElementAtOrDefault(2)),
            versionEnd >= 4 ? CleanVersion(p[3]) : "",
            sourceIndex > 0 ? p[sourceIndex] : "");
    }

    private static string CleanVersion(string? value)
        => value?.TrimStart('>').Trim() ?? "";
}
// kaneki
