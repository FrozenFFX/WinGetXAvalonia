// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Text.Json;
using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Services.Enterprise;

public sealed class FleetReportWriter
{
    public async Task WriteJsonAsync(
        string path,
        IEnumerable<CompanyComputer> computers,
        CancellationToken cancellationToken = default)
    {
        var rows = computers.Select(c => new
        {
            c.Name,
            c.Os,
            c.Cpu,
            c.Ram,
            c.Online,
            c.LastSeen,
            c.PendingUpdates,
            c.CriticalUpdates,
            c.FailedInstallations,
            InstalledApps = c.InstalledApps.ToList()
        });

        var json = JsonSerializer.Serialize(rows, WriteOptions);

        await File.WriteAllTextAsync(path, json, cancellationToken);
    }

    public async Task WriteCsvAsync(
        string path,
        IEnumerable<CompanyComputer> computers,
        CancellationToken cancellationToken = default)
    {
        var lines = new List<string>
        {
            "Name,OS,CPU,RAM,Online,LastSeen,PendingUpdates,CriticalUpdates,FailedInstallations,InstalledApps"
        };

        foreach (var c in computers)
        {
            lines.Add(
                $"{Csv(c.Name)},{Csv(c.Os)},{Csv(c.Cpu)},{Csv(c.Ram)},{c.Online}," +
                $"{c.LastSeen:yyyy-MM-dd HH:mm},{c.PendingUpdates},{c.CriticalUpdates}," +
                $"{c.FailedInstallations},{Csv(string.Join(" | ", c.InstalledApps))}");
        }

        await File.WriteAllTextAsync(
            path,
            string.Join(Environment.NewLine, lines),
            cancellationToken);
    }

    private static string Csv(string value)
        => $"\"{value.Replace("\"", "\"\"")}\"";

    private static readonly JsonSerializerOptions WriteOptions = new()
    {
        WriteIndented = true
    };
}
