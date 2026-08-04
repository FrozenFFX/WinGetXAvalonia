// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using WinGetXAvalonia.Core.Domain.Packages;
using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Tests;

public class SearchEngineTests
{
    private static PackageModel Package(string name, string id)
        => new() { Id = id, Name = name };

    [Fact]
    public void Rank_Contains_PrefersExactOverPrefixAndContains()
    {
        var packages = new[]
        {
            Package("Advanced Git Config", "Dev.GitConfig"),
            Package("Git", "Git.Git"),
            Package("GitHub Desktop", "GitHub.Desktop"),
            Package("My Git Tools", "Me.GitTools")
        };

        var ranked = SearchEngine.Rank(packages, "git").Select(p => p.Id).ToList();

        Assert.Equal(new[] { "Git.Git", "GitHub.Desktop", "Dev.GitConfig", "Me.GitTools" }, ranked);
    }

    [Fact]
    public void Rank_EmptyQuery_PreservesOrder()
    {
        var packages = new[]
        {
            Package("Zed", "Zed.Zed"),
            Package("Alpha", "A.A")
        };

        var ranked = SearchEngine.Rank(packages, "   ").Select(p => p.Id).ToList();

        Assert.Equal(new[] { "Zed.Zed", "A.A" }, ranked);
    }

    [Fact]
    public void Score_Contains_RanksByNameThenId()
    {
        Assert.Equal(100, SearchEngine.Score(Package("Git", "X.Y"), "git"));
        Assert.Equal(95, SearchEngine.Score(Package("Editor", "git"), "git"));
        Assert.Equal(85, SearchEngine.Score(Package("Git Tools", "X.Y"), "git"));
        Assert.Equal(75, SearchEngine.Score(Package("Editor", "git-tools"), "git"));
        Assert.Equal(50, SearchEngine.Score(Package("My Git", "X.Y"), "git"));
        Assert.Equal(40, SearchEngine.Score(Package("Editor", "x-git-y"), "git"));
        Assert.Equal(0, SearchEngine.Score(Package("Editor", "X.Y"), "git"));
    }

    [Fact]
    public void Score_Exact_OnlyMatchesExactNameOrId()
    {
        Assert.Equal(100, SearchEngine.Score(Package("Git", "X.Y"), "git", SearchMatchMode.Exact));
        Assert.Equal(95, SearchEngine.Score(Package("Editor", "git"), "git", SearchMatchMode.Exact));
        Assert.Equal(0, SearchEngine.Score(Package("Git Tools", "X.Y"), "git", SearchMatchMode.Exact));
        Assert.Equal(0, SearchEngine.Score(Package("My Git", "X.Y"), "git", SearchMatchMode.Exact));
    }

    [Fact]
    public void Score_StartsWith_AllowsPrefixButNotContains()
    {
        Assert.Equal(85, SearchEngine.Score(Package("Git Tools", "X.Y"), "git", SearchMatchMode.StartsWith));
        Assert.Equal(75, SearchEngine.Score(Package("Editor", "git-tools"), "git", SearchMatchMode.StartsWith));
        Assert.Equal(0, SearchEngine.Score(Package("My Git", "X.Y"), "git", SearchMatchMode.StartsWith));
    }

    [Fact]
    public void Rank_InstalledFirst_BoostsInstalledPackages()
    {
        var installed = Package("Git", "Git.Git");
        installed.InstallState = PackageInstallState.Installed;

        var packages = new[]
        {
            Package("Git", "X.Y"),
            installed
        };

        var ranked = SearchEngine.Rank(
            packages,
            "git",
            new SearchOptions(InstalledFirst: true));

        Assert.Equal(new[] { "Git.Git", "X.Y" }, ranked.Select(p => p.Id));
    }

    [Fact]
    public void Rank_UpdateAvailableFirst_BoostsUpdatablePackages()
    {
        var updatable = Package("Git", "X.Y");
        updatable.Available = "2.52.0";

        var packages = new[]
        {
            Package("Git", "A.B"),
            updatable
        };

        var ranked = SearchEngine.Rank(
            packages,
            "git",
            new SearchOptions(UpdateAvailableFirst: true));

        Assert.Equal(new[] { "X.Y", "A.B" }, ranked.Select(p => p.Id));
    }

    [Fact]
    public void Rank_FeaturedBoost_PrefersFeaturedOnTies()
    {
        var featured = Package("Git", "A.B");
        featured.IsFeatured = true;

        var packages = new[]
        {
            Package("Git", "Z.Z"),
            featured
        };

        var ranked = SearchEngine.Rank(
            packages,
            "git",
            new SearchOptions(FeaturedBoost: true));

        Assert.Equal(new[] { "A.B", "Z.Z" }, ranked.Select(p => p.Id));
    }

    [Fact]
    public void Rank_OrderingChain_AppliesAllSignals()
    {
        var plain = Package("Git", "Plain");
        var featured = Package("Git", "Featured");
        featured.IsFeatured = true;
        var installed = Package("Git", "Installed");
        installed.InstallState = PackageInstallState.Installed;

        var packages = new[]
        {
            plain,
            featured,
            installed
        };

        var ranked = SearchEngine.Rank(
            packages,
            "git",
            new SearchOptions(
                InstalledFirst: true,
                UpdateAvailableFirst: true,
                FeaturedBoost: true));

        Assert.Equal(new[] { "Featured", "Installed", "Plain" }, ranked.Select(p => p.Id));
    }
}
