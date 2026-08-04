using WinGetXAvalonia.Core.Domain.History;
using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Tests;

public class HistoryGrouperTests
{
    private static readonly DateTime Now = new(2026, 8, 4, 12, 0, 0);

    [Theory]
    [InlineData(0, "Today")]
    [InlineData(-1, "Yesterday")]
    [InlineData(-2, "Last 7 Days")]
    [InlineData(-6, "Last 7 Days")]
    [InlineData(-7, "Older")]
    [InlineData(-30, "Older")]
    public void GroupTitle_ClassifiesDates(int daysAgo, string expected)
        => Assert.Equal(expected, HistoryGrouper.GroupTitle(Now.AddDays(daysAgo), Now));

    private static HistoryEntry Entry(string packageId, DateTime timestamp)
        => new()
        {
            PackageId = packageId,
            PackageName = packageId,
            Operation = "Install",
            Timestamp = timestamp,
            Success = true
        };

    [Fact]
    public void Group_OrdersItemsIntoBuckets()
    {
        var entries = new List<HistoryEntry>
        {
            Entry("today1", Now.AddHours(-1)),
            Entry("today2", Now.AddHours(-2)),
            Entry("yesterday", Now.AddDays(-1)),
            Entry("old1", Now.AddDays(-10))
        };

        var groups = HistoryGrouper.Group(entries, Now);

        Assert.Equal(3, groups.Count);
        Assert.Equal("Today", groups[0].Title);
        Assert.Equal("Yesterday", groups[1].Title);
        Assert.Equal("Older", groups[2].Title);
        Assert.Equal(2, groups[0].Entries.Count);
    }

    [Fact]
    public void Group_Empty_ReturnsNoGroups()
    {
        Assert.Empty(HistoryGrouper.Group(Array.Empty<HistoryEntry>(), Now));
    }
}
