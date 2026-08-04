// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using WinGetXAvalonia.Core.Domain.Packages;
using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Tests;

public class PackageSorterTests
{
    private static PackageModel Package(
        string id,
        string version = "1.0.0",
        string source = "winget",
        string available = "")
        => new() { Id = id, Name = id, Version = version, Source = source, Available = available };

    [Fact]
    public void Sort_ByName_UsesOrdinalIgnoreCase()
    {
        var packages = new[] { Package("beta"), Package("Alpha"), Package("alice") };

        var sorted = PackageSorter.Sort(packages, PackageSortMode.Name).ToList();

        Assert.Equal(new[] { "alice", "Alpha", "beta" }, sorted.Select(p => p.Id));
    }

    [Fact]
    public void Sort_ByVersion_UsesSemanticOrder()
    {
        var packages = new[]
        {
            Package("b", version: "2.9.0"),
            Package("c", version: "2.10.0"),
            Package("a", version: "1.5.0")
        };

        var sorted = PackageSorter.Sort(packages, PackageSortMode.Version).ToList();

        Assert.Equal(new[] { "a", "b", "c" }, sorted.Select(p => p.Id));
    }

    [Fact]
    public void Sort_BySource_OrdersBySource()
    {
        var packages = new[]
        {
            Package("a", source: "msstore"),
            Package("b", source: "winget"),
            Package("c", source: "portable")
        };

        var sorted = PackageSorter.Sort(packages, PackageSortMode.Source).ToList();

        Assert.Equal(new[] { "a", "c", "b" }, sorted.Select(p => p.Id));
    }

    [Fact]
    public void Sort_ByAvailable_OrdersEmptyFirst()
    {
        var packages = new[]
        {
            Package("a", available: "2.52.0"),
            Package("b", available: ""),
            Package("c", available: "2.10.0")
        };

        var sorted = PackageSorter.Sort(packages, PackageSortMode.Available).ToList();

        Assert.Equal(new[] { "b", "c", "a" }, sorted.Select(p => p.Id));
    }
}
