using WinGetXAvalonia.Core.Domain.Packages;

namespace WinGetXAvalonia.Tests;

public class PackageVersionComparerTests
{
    [Theory]
    [InlineData("2.10.0", "2.9.0", 1)]
    [InlineData("2.9.0", "2.10.0", -1)]
    [InlineData("1.0.0", "1.0.0", 0)]
    [InlineData("1.0", "1.0.0", 0)]
    [InlineData("2.5.0", "2.5", 0)]
    [InlineData("0.9.9", "1.0.0", -1)]
    [InlineData("1.0.0-beta", "1.0.0", 0)]
    [InlineData("", "1.0.0", -1)]
    [InlineData("1.0.0", "", 1)]
    [InlineData("", "", 0)]
    [InlineData(null, "1.0.0", -1)]
    [InlineData(null, null, 0)]
    public void CompareVersions_OrdersVersions(string? left, string? right, int expected)
        => Assert.Equal(expected, Math.Sign(PackageVersionComparer.CompareVersions(left, right)));

    [Fact]
    public void IsNewer_ComparesCandidates()
    {
        Assert.True(PackageVersionComparer.IsNewer("2.52.0", "2.51.0"));
        Assert.False(PackageVersionComparer.IsNewer("2.51.0", "2.52.0"));
        Assert.True(PackageVersionComparer.IsSameOrNewer("2.52.0", "2.52.0"));
    }

    [Fact]
    public void Instance_SortsVersionsSemantically()
    {
        var versions = new[] { "2.9.0", "2.10.0", "1.5.0" };

        var sorted = versions.OrderBy(v => v, PackageVersionComparer.Instance).ToList();

        Assert.Equal(new[] { "1.5.0", "2.9.0", "2.10.0" }, sorted);
    }
}
