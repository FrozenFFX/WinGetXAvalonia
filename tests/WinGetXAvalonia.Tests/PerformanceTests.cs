// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Core;
using WinGetXAvalonia.Services.Performance;

namespace WinGetXAvalonia.Tests;

public class PerformanceTests
{
    [Fact]
    public void Capture_ReturnsNonZeroMetrics()
    {
        var service = new PerformanceService();

        var snapshot = service.Capture();

        Assert.True(snapshot.Memory > 0);
        Assert.True(snapshot.Threads > 0);
        Assert.True(snapshot.Handles > 0);
        Assert.NotEqual(TimeSpan.Zero, snapshot.CpuTime);
    }

    [Fact]
    public void Snapshot_IsImmutable()
    {
        var snapshot = new PerformanceSnapshot
        {
            Memory = 1024,
            Threads = 4,
            Handles = 8,
            CpuTime = TimeSpan.FromSeconds(1)
        };

        Assert.Equal(1024, snapshot.Memory);
        Assert.Equal(4, snapshot.Threads);
        Assert.Equal(8, snapshot.Handles);
        Assert.Equal(TimeSpan.FromSeconds(1), snapshot.CpuTime);
    }
}

public class PackageCacheServiceTests
{
    [Fact]
    public void StoreThenTryGet_ReturnsStoredDetails()
    {
        var cache = new PackageCacheService();

        cache.Store("Notepad++.Notepad++", new PackageDetails { Name = "Notepad++" });

        Assert.True(cache.TryGet("Notepad++.Notepad++", out var details));
        Assert.Equal("Notepad++", details.Name);
    }

    [Fact]
    public void TryGet_MissingId_ReturnsFalse()
    {
        var cache = new PackageCacheService();

        Assert.False(cache.TryGet("missing", out _));
    }

    [Fact]
    public void Store_OverwritesExistingId()
    {
        var cache = new PackageCacheService();

        cache.Store("pkg", new PackageDetails { Name = "First" });
        cache.Store("pkg", new PackageDetails { Name = "Second" });

        Assert.True(cache.TryGet("pkg", out var details));
        Assert.Equal("Second", details.Name);
    }

    [Fact]
    public void Clear_RemovesAllEntries()
    {
        var cache = new PackageCacheService();

        cache.Store("pkg", new PackageDetails { Name = "First" });
        cache.Clear();

        Assert.False(cache.TryGet("pkg", out _));
    }
}
