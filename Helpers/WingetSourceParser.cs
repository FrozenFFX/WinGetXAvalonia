// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Text.RegularExpressions;
using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Helpers;

public static class WingetSourceParser
{
    public static IReadOnlyList<WingetSource> Parse(string output)
    {
        var list = new List<WingetSource>();

        foreach (var line in output.Split(Environment.NewLine))
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            if (line.StartsWith("Name"))
                continue;

            var parts = Regex.Split(line.Trim(), @"\s{2,}");

            if (parts.Length < 3)
                continue;

            list.Add(new WingetSource
            {
                Name = parts[0],
                Type = parts[1],
                Argument = parts[2],
                Enabled = true
            });
        }

        return list;
    }
}
