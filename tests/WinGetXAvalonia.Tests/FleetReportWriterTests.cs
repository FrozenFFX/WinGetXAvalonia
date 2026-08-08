// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Text.Json;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Enterprise;

namespace WinGetXAvalonia.Tests;

public class FleetReportWriterTests
{
    private static string TempDir()
    {
        var dir = Path.Combine(Path.GetTempPath(), "WinGetXAvalonia.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        return dir;
    }

    [Fact]
    public async Task WriteJson_WritesValidJson()
    {
        var file = Path.Combine(TempDir(), "report.json");

        var writer = new FleetReportWriter();
        await writer.WriteJsonAsync(file, SampleComputers());

        using var doc = JsonDocument.Parse(await File.ReadAllTextAsync(file));
        Assert.Equal(2, doc.RootElement.GetArrayLength());
    }

    [Fact]
    public async Task WriteCsv_WritesHeaderAndRows()
    {
        var file = Path.Combine(TempDir(), "report.csv");

        var writer = new FleetReportWriter();
        await writer.WriteCsvAsync(file, SampleComputers());

        var lines = await File.ReadAllLinesAsync(file);
        Assert.Equal(3, lines.Length);
        Assert.StartsWith("Name,OS,CPU", lines[0]);
        Assert.Contains("IT-DESKTOP-01", lines[1]);
    }

    private static IReadOnlyList<CompanyComputer> SampleComputers()
        => new List<CompanyComputer>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "IT-DESKTOP-01",
                Os = "Windows 11",
                Cpu = "i7",
                Ram = "32 GB",
                Online = true,
                PendingUpdates = 3,
                CriticalUpdates = 1,
                FailedInstallations = 0
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "IT-LAPTOP-02",
                Os = "Windows 11",
                Cpu = "i5",
                Ram = "16 GB",
                Online = false,
                PendingUpdates = 0,
                CriticalUpdates = 0,
                FailedInstallations = 2
            }
        };
}
