using WinGetXAvalonia.Core.Domain.Packages;
using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Tests;

public class PackageStateCalculatorTests
{
    private static PackageModel Package(string id = "Git.Git")
        => new() { Id = id, Name = "Git" };

    [Fact]
    public void Calculate_NullIndex_ReturnsNotInstalled()
    {
        var state = PackageStateCalculator.Calculate(Package(), null);

        Assert.Equal(PackageInstallState.NotInstalled, state.InstallState);
        Assert.Equal("", state.InstalledVersion);
        Assert.Equal("", state.AvailableVersion);
    }

    [Fact]
    public void Calculate_EmptyIndex_ReturnsNotInstalled()
    {
        var state = PackageStateCalculator.Calculate(Package(), new PackageIndex());

        Assert.Equal(PackageInstallState.NotInstalled, state.InstallState);
    }

    [Fact]
    public void Calculate_InstalledOnly_ReturnsInstalledState()
    {
        var index = new PackageIndex();
        index.AddInstalled(new[]
        {
            new InstalledPackage { Id = "Git.Git", Version = "2.51.0", Source = "winget" }
        });

        var state = PackageStateCalculator.Calculate(Package(), index);

        Assert.Equal(PackageInstallState.Installed, state.InstallState);
        Assert.Equal("2.51.0", state.InstalledVersion);
        Assert.Equal("winget", state.Source);
        Assert.False(state.HasUpdate);
    }

    [Fact]
    public void Calculate_WithUpdate_SetsHasUpdate()
    {
        var index = new PackageIndex();
        index.AddInstalled(new[] { new InstalledPackage { Id = "Git.Git", Version = "2.51.0" } });
        index.AddUpdates(new[] { new UpdatePackage { Id = "Git.Git", Available = "2.52.0" } });

        var state = PackageStateCalculator.Calculate(Package(), index);

        Assert.Equal(PackageInstallState.Installed, state.InstallState);
        Assert.Equal("2.52.0", state.AvailableVersion);
        Assert.True(state.HasUpdate);
    }

    [Fact]
    public void Apply_UpdateWithoutInstall_PreservesSearchVersion()
    {
        var index = new PackageIndex();
        index.AddUpdates(new[] { new UpdatePackage { Id = "Git.Git", Available = "2.52.0" } });

        var package = Package();
        package.Version = "2.51.0";

        PackageStateCalculator.Apply(package, index);

        Assert.Equal(PackageInstallState.NotInstalled, package.InstallState);
        Assert.Equal("2.51.0", package.Version);
        Assert.Equal("2.52.0", package.Available);
    }

    [Fact]
    public void Apply_Installed_OverwritesVersionAndFillsSource()
    {
        var index = new PackageIndex();
        index.AddInstalled(new[]
        {
            new InstalledPackage { Id = "Git.Git", Version = "2.51.0", Source = "winget" }
        });

        var package = Package();

        PackageStateCalculator.Apply(package, index);

        Assert.Equal(PackageInstallState.Installed, package.InstallState);
        Assert.Equal("2.51.0", package.Version);
        Assert.Equal("winget", package.Source);
    }

    [Fact]
    public void Apply_NullIndex_MarksNotInstalled()
    {
        var package = Package();

        PackageStateCalculator.Apply(package, null);

        Assert.Equal(PackageInstallState.NotInstalled, package.InstallState);
    }
}
