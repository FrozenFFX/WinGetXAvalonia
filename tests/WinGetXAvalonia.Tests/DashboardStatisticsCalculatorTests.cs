// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using WinGetXAvalonia.Core.Domain.Dashboard;

namespace WinGetXAvalonia.Tests;

public class DashboardStatisticsCalculatorTests
{
    [Fact]
    public void Calculate_CountsRawLinesAndFavorites()
    {
        var stats = DashboardStatisticsCalculator.Calculate(
            installedLines: new[] { "a", "b", "c" },
            updateLines: new[] { "a" },
            sourceLines: new[] { "winget", "msstore" },
            favoriteCount: 4);

        Assert.Equal(3, stats.InstalledCount);
        Assert.Equal(1, stats.UpdateCount);
        Assert.Equal(2, stats.SourceCount);
        Assert.Equal(4, stats.FavoriteCount);
    }

    [Fact]
    public void Calculate_EmptyInputs_ReturnsZeroCounts()
    {
        var stats = DashboardStatisticsCalculator.Calculate(
            installedLines: Array.Empty<string>(),
            updateLines: Array.Empty<string>(),
            sourceLines: Array.Empty<string>(),
            favoriteCount: 0);

        Assert.Equal(0, stats.InstalledCount);
        Assert.Equal(0, stats.UpdateCount);
        Assert.Equal(0, stats.SourceCount);
        Assert.Equal(0, stats.FavoriteCount);
    }
}
