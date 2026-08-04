// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Core.Domain.History;

public static class HistoryGrouper
{
    public static string GroupTitle(DateTime timestamp, DateTime now)
    {
        DateTime date = timestamp.Date;
        DateTime today = now.Date;

        if (date == today)
            return "Today";

        if (date == today.AddDays(-1))
            return "Yesterday";

        if (date > today.AddDays(-7))
            return "Last 7 Days";

        return "Older";
    }

    public static List<HistoryGroup> Group(IEnumerable<HistoryEntry> entries, DateTime now)
    {
        var groups = new List<HistoryGroup>();
        var current = new List<HistoryEntry>();
        string? currentTitle = null;

        foreach (var entry in entries)
        {
            string title = GroupTitle(entry.Timestamp, now);

            if (currentTitle != title)
            {
                if (current.Count > 0)
                    groups.Add(new HistoryGroup(currentTitle!, current));

                current = new List<HistoryEntry>();
                currentTitle = title;
            }

            current.Add(entry);
        }

        if (current.Count > 0)
            groups.Add(new HistoryGroup(currentTitle!, current));

        return groups;
    }
}

public sealed record HistoryGroup(string Title, IReadOnlyList<HistoryEntry> Entries);
