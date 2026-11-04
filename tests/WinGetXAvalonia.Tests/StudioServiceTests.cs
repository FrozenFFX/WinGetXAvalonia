// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Studio;

namespace WinGetXAvalonia.Tests;

public class StudioServiceTests
{
    private readonly StudioService _service = new();

    [Fact]
    public async Task GetToolsAsync_ReturnsNonEmptyCatalog()
    {
        var tools = await _service.GetToolsAsync();

        Assert.NotEmpty(tools);
    }

    [Fact]
    public async Task GetToolsAsync_EveryToolHasMetadata()
    {
        var tools = await _service.GetToolsAsync();

        Assert.All(tools, t =>
        {
            Assert.False(string.IsNullOrWhiteSpace(t.Id));
            Assert.False(string.IsNullOrWhiteSpace(t.Icon));
            Assert.False(string.IsNullOrWhiteSpace(t.Title));
            Assert.False(string.IsNullOrWhiteSpace(t.Description));
            Assert.False(string.IsNullOrWhiteSpace(t.Category));
        });
    }

    [Fact]
    public async Task GetToolsAsync_IdsAreUnique()
    {
        var tools = await _service.GetToolsAsync();

        Assert.Equal(tools.Count, tools.Select(t => t.Id).Distinct().Count());
    }

    [Fact]
    public async Task GetToolsAsync_IncludesExpectedTools()
    {
        var tools = await _service.GetToolsAsync();

        Assert.Contains(tools, t => t.Id == "diagnostics");
        Assert.Contains(tools, t => t.Id == "founder");
    }

    [Fact]
    public async Task GetToolsAsync_UnimplementedToolsAreMarkedComingSoon()
    {
        var tools = await _service.GetToolsAsync();

        Assert.Contains(tools, t => t.Status == StudioToolStatus.Ready);
        Assert.Contains(tools, t => t.Status == StudioToolStatus.ComingSoon);
        Assert.DoesNotContain(tools, t => t.Status == StudioToolStatus.Experimental);
        Assert.All(
            tools.Where(t => t.Id is "reports" or "queue" or "plugins" or "founder"),
            tool => Assert.Equal(StudioToolStatus.ComingSoon, tool.Status));
    }
}
