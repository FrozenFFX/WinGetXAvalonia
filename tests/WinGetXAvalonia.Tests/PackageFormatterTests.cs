// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using WinGetXAvalonia.Core.Domain.Packages;
using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Tests;

public class PackageFormatterTests
{
    [Theory]
    [InlineData(PackageInstallState.NotInstalled, false, "Install")]
    [InlineData(PackageInstallState.Installing, false, "Installing...")]
    [InlineData(PackageInstallState.Updating, false, "Updating...")]
    [InlineData(PackageInstallState.Installed, true, "Update")]
    [InlineData(PackageInstallState.Installed, false, "Installed")]
    public void InstallButtonText_MatchesState(PackageInstallState state, bool hasUpdate, string expected)
        => Assert.Equal(expected, PackageFormatter.InstallButtonText(state, hasUpdate));

    [Fact]
    public void DisplayLabels_FormatVersions()
    {
        Assert.Equal("Installed 2.51.0", PackageFormatter.InstalledVersion("2.51.0"));
        Assert.Equal("Available 2.52.0", PackageFormatter.AvailableVersion("2.52.0"));
        Assert.Equal("Update 2.52.0", PackageFormatter.UpdateMarker("2.52.0"));
        Assert.Equal("Version 2.52.0", PackageFormatter.Version("2.52.0"));
        Assert.Equal("Not Installed", PackageFormatter.NotInstalledText);
    }

    [Theory]
    [InlineData(null, "-")]
    [InlineData("", "-")]
    [InlineData("   ", "-")]
    [InlineData("2.51.0", "2.51.0")]
    public void ValueOrDash_FallsBackToDash(string? value, string expected)
        => Assert.Equal(expected, PackageFormatter.ValueOrDash(value));
}
