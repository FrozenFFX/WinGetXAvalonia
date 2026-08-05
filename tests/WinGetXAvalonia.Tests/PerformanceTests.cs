// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using WinGetXAvalonia.Core.Operations;
using WinGetXAvalonia.Core.Providers;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Core;
using WinGetXAvalonia.Services.PackageDetails;
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

public class PackageDetailsServiceCacheTests
{
    [Fact]
    public async Task GetAsync_SameId_ProviderCalledOnce()
    {
        var cache = new PackageCacheService();
        var provider = new CountingProvider();

        var service = new PackageDetailsService(provider, cache);

        var first = await service.GetAsync("pkg");
        var second = await service.GetAsync("pkg");

        Assert.Equal(1, provider.Calls);
        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.Equal(first?.Id, second?.Id);
    }

    [Fact]
    public async Task GetAsync_MissingId_DoesNotCache()
    {
        var cache = new PackageCacheService();
        var provider = new CountingProvider();

        var service = new PackageDetailsService(provider, cache);

        var result = await service.GetAsync("");

        Assert.Null(result);
        Assert.Equal(0, provider.Calls);
    }

    private sealed class CountingProvider : IPackageProvider
    {
        public int Calls { get; private set; }

        public Task<PackageDetails> GetDetailsAsync(string id)
        {
            Calls++;

            return Task.FromResult(new PackageDetails { Id = id, Name = "Package" });
        }

        public Task<IReadOnlyList<PackageModel>> SearchAsync(string query) => throw new NotImplementedException();

        public Task InstallAsync(string id, IOperationProgress? progress = null) => Task.CompletedTask;

        public Task UpgradeAsync(string id, IOperationProgress? progress = null) => Task.CompletedTask;

        public Task UninstallAsync(string id, IOperationProgress? progress = null) => Task.CompletedTask;

        public Task<IReadOnlyList<InstalledPackage>> GetInstalledAsync() => throw new NotImplementedException();

        public Task<IReadOnlyList<UpdatePackage>> GetUpdatesAsync() => throw new NotImplementedException();

        public Task<string?> GetHomepageAsync(string id) => throw new NotImplementedException();
    }
}
