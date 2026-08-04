// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using WinGetXAvalonia.Core.Domain.Package;
using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Tests;

public class PackageHealthCalculatorTests
{
    private static PackageModel InstalledPackage(string source, string version = "2.51.0", string available = "")
        => new()
        {
            Id = "Git.Git",
            Name = "Git",
            Source = source,
            Version = version,
            Available = available,
            InstallState = PackageInstallState.Installed
        };

    [Fact]
    public void Calculate_WingetUpToDate_ReturnsPerfectScore()
    {
        var health = PackageHealthCalculator.Calculate(InstalledPackage("winget"));

        Assert.Equal(100, health.Score);
        Assert.Equal(SourceTrustLevel.High, health.SourceTrust);
        Assert.Equal(UpdateStatus.UpToDate, health.UpdateStatus);
        Assert.True(health.IsPublisherVerified);
        Assert.Equal("High", health.SourceTrustLabel);
        Assert.Equal("✔ Latest", health.LatestVersionLabel);
        Assert.Equal("Good", health.UpdateFrequencyLabel);
        Assert.Equal("✔ Verified", health.PublisherLabel);
    }

    [Fact]
    public void Calculate_MsStore_IsHighTrust()
    {
        var health = PackageHealthCalculator.Calculate(InstalledPackage("msstore"));

        Assert.Equal(100, health.Score);
        Assert.Equal(SourceTrustLevel.High, health.SourceTrust);
        Assert.True(health.IsPublisherVerified);
    }

    [Fact]
    public void Calculate_UpdatePending_LowersScore()
    {
        var health = PackageHealthCalculator.Calculate(
            InstalledPackage("winget", available: "2.52.0"));

        Assert.Equal(93, health.Score);
        Assert.Equal(UpdateStatus.UpdateAvailable, health.UpdateStatus);
        Assert.Equal("Update available (2.52.0)", health.LatestVersionLabel);
    }

    [Fact]
    public void Calculate_PortableSource_IsMediumTrust()
    {
        var health = PackageHealthCalculator.Calculate(InstalledPackage("portable"));

        Assert.Equal(92, health.Score);
        Assert.Equal(SourceTrustLevel.Medium, health.SourceTrust);
        Assert.Equal("Medium", health.SourceTrustLabel);
        Assert.False(health.IsPublisherVerified);
    }

    [Fact]
    public void Calculate_UnknownSource_IsLowTrust()
    {
        var health = PackageHealthCalculator.Calculate(InstalledPackage("community"));

        Assert.Equal(82, health.Score);
        Assert.Equal(SourceTrustLevel.Low, health.SourceTrust);
        Assert.Equal("Low", health.SourceTrustLabel);
        Assert.False(health.IsPublisherVerified);
    }

    [Fact]
    public void Calculate_EmptySource_IsUnknownTrust()
    {
        var health = PackageHealthCalculator.Calculate(InstalledPackage(""));

        Assert.Equal(82, health.Score);
        Assert.Equal("Unknown", health.SourceTrustLabel);
    }

    [Theory]
    [InlineData("0.9.1")]
    [InlineData("2.0.0-beta")]
    [InlineData("2.0.0-alpha.1")]
    [InlineData("2.0.0-preview")]
    [InlineData("2.0.0-rc")]
    public void Calculate_PrereleaseVersion_IsDeveloping(string version)
    {
        var health = PackageHealthCalculator.Calculate(InstalledPackage("winget", version));

        Assert.Equal("Developing", health.UpdateFrequencyLabel);
        Assert.Equal(95, health.Score);
    }

    [Fact]
    public void Calculate_NotInstalled_IsNotInstalled()
    {
        var package = InstalledPackage("winget");
        package.InstallState = PackageInstallState.NotInstalled;

        var health = PackageHealthCalculator.Calculate(package);

        Assert.Equal(UpdateStatus.NotInstalled, health.UpdateStatus);
        Assert.Equal("Not installed", health.LatestVersionLabel);
    }

    [Fact]
    public void Calculate_CombinedPenalties_ClampsToRange()
    {
        var health = PackageHealthCalculator.Calculate(
            InstalledPackage("community", version: "0.5.0", available: "0.6.0"));

        Assert.Equal(70, health.Score);
    }

    [Fact]
    public void None_IsZeroScorePlaceholder()
    {
        Assert.Equal(0, PackageHealth.None.Score);
        Assert.Equal("—", PackageHealth.None.SourceTrustLabel);
    }
}
