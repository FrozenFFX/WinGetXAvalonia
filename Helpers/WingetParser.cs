// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Text.RegularExpressions;
using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Helpers;

public static class WingetParser
{
    public static List<InstalledPackage> ParseInstalled(List<string> lines)
    {
        lines = lines
            .Where(x => !x.StartsWith("---"))
            .ToList();

        var list = new List<InstalledPackage>();

        foreach (string line in lines.Skip(1))
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
        lines = lines
            .Where(x => !x.StartsWith("---"))
            .ToList();

        var list = new List<UpdatePackage>();

        foreach (string line in lines.Skip(1))
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
        lines = lines
            .Where(x => !x.StartsWith("---"))
            .ToList();

        var list = new List<PackageModel>();

        foreach (string line in lines.Skip(1))
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

    public static PackageDetails ParseDetails(IEnumerable<string> lines)
    {
        var details = new PackageDetails();

        foreach (string line in lines)
        {
            var match = Regex.Match(line.Trim(), @"^([^:]+):\s*(.*)$");

            if (!match.Success)
                continue;

            string key = match.Groups[1].Value.Trim();
            string value = match.Groups[2].Value.Trim();

            switch (key)
            {
                case "Name":
                    details = details with { Name = value };
                    break;
                case "Id":
                    details = details with { Id = value };
                    break;
                case "Version":
                    details = details with { Version = value };
                    break;
                case "Latest Version":
                    details = details with { LatestVersion = value };
                    break;
                case "Source":
                    details = details with { Source = value };
                    break;
                case "Publisher":
                    details = details with { Publisher = value };
                    break;
                case "Description":
                    details = details with { Description = value };
                    break;
                case "Homepage":
                    if (Uri.TryCreate(value, UriKind.Absolute, out _))
                        details = details with { Homepage = value };
                    break;
                case "License":
                    details = details with { License = value };
                    break;
                case "ReleaseNotes":
                    details = details with { ReleaseNotes = value };
                    break;
                case "Architecture":
                    details = details with { Architecture = value };
                    break;
                case "Scope":
                    details = details with { Scope = value };
                    break;
                case "InstallerType":
                    details = details with { InstallerType = value };
                    break;
                case "InstallerUrl":
                    details = details with { InstallerUrl = value };
                    break;
                case "InstallerSha256":
                    details = details with { InstallerSha256 = value };
                    break;
                case "Dependencies":
                    details = details with { Dependencies = value.Split(new[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries).ToList() };
                    break;
                case "Tags":
                    details = details with { Tags = value.Split(new[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries).ToList() };
                    break;
            }
        }

        return details;
    }

    private static (string Name, string? Id, string Version, string Available, string Source) SplitRow(string line)
    {
        line = line.Replace('\t', ' ');

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
    {
        if (string.IsNullOrWhiteSpace(value))
            return "";

        return value
            .Trim()
            .TrimStart('>')
            .Trim();
    }
}
