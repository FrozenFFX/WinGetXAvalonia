using WinGetXAvalonia.Core.Domain.Package;
using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Tests;

public class PackageHealthSummaryCalculatorTests
{
    private static InstalledPackage Installed(string id, string source, string version = "2.51.0")
        => new()
        {
            Id = id,
            Name = id,
            Version = version,
            Source = source
        };

    private static UpdatePackage Update(string id, string available)
        => new()
        {
            Id = id,
            Name = id,
            Version = "2.51.0",
            Available = available,
            Source = "winget"
        };

    private static HistoryEntry History(bool success)
        => new()
        {
            Timestamp = DateTime.Now,
            Operation = "install",
            PackageName = "Git",
            PackageId = "Git.Git",
            Version = "2.51.0",
            Success = success,
            Message = success ? null : "error"
        };

    [Fact]
    public void Calculate_AllHealthy_CountsHealthy()
    {
        var installed = new[]
        {
            Installed("Git.Git", "winget"),
            Installed("VSCode", "winget"),
            Installed("Spotify", "msstore")
        };

        var summary = PackageHealthSummaryCalculator.Calculate(installed, [], []);

        Assert.Equal(3, summary.Total);
        Assert.Equal(3, summary.Healthy);
        Assert.Equal(0, summary.Outdated);
        Assert.Equal(0, summary.Deprecated);
        Assert.Equal(0, summary.UnknownSource);
        Assert.Equal(0, summary.Failed);
    }

    [Fact]
    public void Calculate_MixedPackages_BucketsCorrectly()
    {
        var installed = new[]
        {
            Installed("Git.Git", "winget"),
            Installed("OldApp", "winget", version: "1.0.0"),
            Installed("BetaApp", "community", version: "0.5.0"),
            Installed("MysteryApp", "")
        };

        var updates = new[]
        {
            Update("OldApp", "1.2.0")
        };

        var summary = PackageHealthSummaryCalculator.Calculate(installed, updates, []);

        Assert.Equal(4, summary.Total);
        Assert.Equal(2, summary.Healthy);
        Assert.Equal(1, summary.Outdated);
        Assert.Equal(1, summary.Deprecated);
        Assert.Equal(1, summary.UnknownSource);
    }

    [Fact]
    public void Calculate_UpdateMatchesId_CaseInsensitive()
    {
        var installed = new[] { Installed("Git.Git", "winget") };
        var updates = new[] { Update("git.git", "2.52.0") };

        var summary = PackageHealthSummaryCalculator.Calculate(installed, updates, []);

        Assert.Equal(1, summary.Outdated);
        Assert.Equal(0, summary.Healthy);
    }

    [Fact]
    public void Calculate_FailedHistory_CountsFailed()
    {
        var history = new[]
        {
            History(true),
            History(false),
            History(false)
        };

        var summary = PackageHealthSummaryCalculator.Calculate([], [], history);

        Assert.Equal(2, summary.Failed);
    }

    [Fact]
    public void Calculate_EmptyInputs_ReturnsEmptySummary()
    {
        var summary = PackageHealthSummaryCalculator.Calculate([], [], []);

        Assert.Equal(0, summary.Total);
        Assert.Equal(0, summary.Healthy);
        Assert.Equal(0, summary.Outdated);
        Assert.Equal(0, summary.Deprecated);
        Assert.Equal(0, summary.UnknownSource);
        Assert.Equal(0, summary.Failed);
    }
}
