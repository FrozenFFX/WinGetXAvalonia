// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using WinGetXAvalonia.Core.Domain.Packages;
using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Tests;

public class PackageFilterTests
{
    private static PackageModel Package(string name, string source)
        => new() { Id = name, Name = name, Source = source };

    [Fact]
    public void IsMatch_ModeAll_IgnoresSource()
    {
        Assert.True(PackageFilter.IsMatch(Package("Git", "winget"), null, PackageFilterMode.All));
        Assert.True(PackageFilter.IsMatch(Package("Spotify", "msstore"), null, PackageFilterMode.All));
    }

    [Theory]
    [InlineData(PackageFilterMode.Winget, "winget", true)]
    [InlineData(PackageFilterMode.Winget, "msstore", false)]
    [InlineData(PackageFilterMode.Winget, "winget-source", true)]
    [InlineData(PackageFilterMode.MsStore, "msstore", true)]
    [InlineData(PackageFilterMode.MsStore, "winget", false)]
    [InlineData(PackageFilterMode.Portable, "portable", true)]
    [InlineData(PackageFilterMode.Portable, "winget", false)]
    public void IsMatch_FiltersBySource(PackageFilterMode mode, string source, bool expected)
        => Assert.Equal(expected, PackageFilter.IsMatch(Package("Git", source), null, mode));

    [Fact]
    public void IsMatch_SearchText_MatchesNameOrId()
    {
        var package = Package("Git", "winget");

        Assert.True(PackageFilter.IsMatch(package, "git", PackageFilterMode.All));
        Assert.True(PackageFilter.IsMatch(package, "GIT", PackageFilterMode.All));
        Assert.False(PackageFilter.IsMatch(package, "docker", PackageFilterMode.All));
    }

    [Fact]
    public void Apply_CombinesTextAndSource()
    {
        var packages = new List<PackageModel>
        {
            Package("Git", "winget"),
            Package("Spotify", "msstore"),
            Package("GitHub CLI", "winget")
        };

        var result = PackageFilter.Apply(packages, "git", PackageFilterMode.Winget).ToList();

        Assert.Equal(2, result.Count);
    }
}
