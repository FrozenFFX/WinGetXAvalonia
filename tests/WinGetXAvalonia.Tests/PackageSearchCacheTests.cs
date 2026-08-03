// ==========================================================
// WinGetX - https://github.com/FrozenFFX
// ==========================================================
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Core;

namespace WinGetXAvalonia.Tests;

// kaneki
public class PackageSearchCacheTests
{
    [Fact]
    public void Set_ThenTryGet_ReturnsResults()
    {
        var cache = new PackageSearchCache();
        cache.Set("git", new List<PackageModel> { new() { Id = "Git.Git", Name = "Git" } });

        Assert.True(cache.TryGet("git", out var results));
        Assert.NotNull(results);
        Assert.Single(results);
        Assert.Equal("Git.Git", results[0].Id);
    }

    [Fact]
    public void TryGet_UnknownQuery_ReturnsFalse()
    {
        var cache = new PackageSearchCache();

        Assert.False(cache.TryGet("git", out _));
    }

    [Fact]
    public void TryGet_IsCaseInsensitive()
    {
        var cache = new PackageSearchCache();
        cache.Set("GIT", new List<PackageModel> { new() { Id = "Git.Git" } });

        Assert.True(cache.TryGet("git", out _));
    }

    [Fact]
    public void All_FlattensEveryEntry()
    {
        var cache = new PackageSearchCache();
        cache.Set("a", new List<PackageModel> { new() { Name = "A" }, new() { Name = "B" } });
        cache.Set("b", new List<PackageModel> { new() { Name = "C" } });

        Assert.Equal(3, cache.All().Count);
    }

    [Fact]
    public void Clear_RemovesAllEntries()
    {
        var cache = new PackageSearchCache();
        cache.Set("a", new List<PackageModel> { new() { Name = "A" } });
        cache.Clear();

        Assert.False(cache.TryGet("a", out _));
    }
}
// kaneki
