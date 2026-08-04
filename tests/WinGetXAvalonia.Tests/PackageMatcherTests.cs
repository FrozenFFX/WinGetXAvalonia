// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using WinGetXAvalonia.Core.Domain.Packages;
using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Tests;

public class PackageMatcherTests
{
    private static PackageModel Package(string id)
        => new() { Id = id, Name = id };

    [Fact]
    public void FindPreferred_ReturnsExactIdMatch()
    {
        var results = new List<PackageModel> { Package("Foo.Bar"), Package("Git.Git") };

        var found = PackageMatcher.FindPreferred(results, "git.git");

        Assert.Same(results[1], found);
    }

    [Fact]
    public void FindPreferred_NoExactMatch_ReturnsFirst()
    {
        var results = new List<PackageModel> { Package("Foo.Bar"), Package("Baz.Qux") };

        var found = PackageMatcher.FindPreferred(results, "Git.Git");

        Assert.Same(results[0], found);
    }

    [Fact]
    public void FindPreferred_Empty_ReturnsNull()
    {
        Assert.Null(PackageMatcher.FindPreferred(Array.Empty<PackageModel>(), "Git.Git"));
    }

    [Fact]
    public void MatchesId_IgnoresCase()
    {
        Assert.True(PackageMatcher.MatchesId(Package("Git.Git"), "git.git"));
        Assert.False(PackageMatcher.MatchesId(Package("Git.Git"), "Foo.Bar"));
    }
}
