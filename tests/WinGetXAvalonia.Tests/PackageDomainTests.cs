using WinGetXAvalonia.Core.Domain.Package;

namespace WinGetXAvalonia.Tests;

public class PackageDomainTests
{
    [Fact]
    public void Identity_DisplayName_IncludesSource()
    {
        var identity = new PackageIdentity("Microsoft.VisualStudioCode", "winget");

        Assert.Equal("Microsoft.VisualStudioCode (winget)", identity.DisplayName);
    }

    [Fact]
    public void Identity_DisplayName_WithoutSource_UsesId()
    {
        var identity = new PackageIdentity("Git.Git", "");

        Assert.Equal("Git.Git", identity.DisplayName);
    }

    [Fact]
    public void Identity_None_IsEmpty()
    {
        Assert.True(PackageIdentity.None.IsEmpty);
        Assert.Equal("", PackageIdentity.None.DisplayName);
    }

    [Fact]
    public void Identity_Matches_IsCaseInsensitive()
    {
        var identity = new PackageIdentity("Microsoft.VisualStudioCode", "winget");

        Assert.True(identity.Matches("microsoft.visualstudiocode", "WINGET"));
        Assert.False(identity.Matches("microsoft.visualstudiocode", "msstore"));
    }

    [Fact]
    public void Version_HasUpdate_WhenAvailableIsNewer()
    {
        var version = new PackageVersion("1.0.0", "1.1.0");

        Assert.True(version.HasUpdate);
        Assert.True(version.IsInstalled);
        Assert.Equal("1.0.0", version.InstalledOrDash);
    }

    [Fact]
    public void Version_HasUpdate_False_WhenSameOrOlder()
    {
        Assert.False(new PackageVersion("1.1.0", "1.0.0").HasUpdate);
        Assert.False(new PackageVersion("1.0.0", "").HasUpdate);
    }

    [Fact]
    public void Version_NotInstalled_ShowsDash()
    {
        var version = PackageVersion.None;

        Assert.False(version.IsInstalled);
        Assert.Equal("—", version.InstalledOrDash);
    }

    [Fact]
    public void Publisher_Unknown_WhenNameEmpty()
    {
        Assert.True(PackagePublisher.Unknown.IsEmpty);
        Assert.Equal("Unknown", PackagePublisher.Unknown.DisplayName);
        Assert.Equal("Not verified", PackagePublisher.Unknown.TrustLabel);
    }

    [Fact]
    public void Publisher_Verified_ShowsVerifiedLabel()
    {
        var publisher = new PackagePublisher("Microsoft", true);

        Assert.Equal("Microsoft", publisher.DisplayName);
        Assert.Equal("✔ Verified", publisher.TrustLabel);
    }

    [Fact]
    public void Manifest_None_IsEmpty()
    {
        Assert.True(PackageManifest.None.IsEmpty);
        Assert.Equal("", PackageManifest.None.Homepage);
    }

    [Fact]
    public void Source_TrustLabel_MapsTrustLevels()
    {
        Assert.Equal("High", new PackageSource("winget", SourceTrustLevel.High).TrustLabel);
        Assert.Equal("Medium", new PackageSource("portable", SourceTrustLevel.Medium).TrustLabel);
        Assert.Equal("Low", new PackageSource("community", SourceTrustLevel.Low).TrustLabel);
        Assert.Equal("Unknown", PackageSource.Unknown.TrustLabel);
    }

    [Fact]
    public void Reference_Create_BuildsIdentity()
    {
        var reference = PackageReference.Create("Microsoft.VisualStudioCode", "winget", "1.0.0");

        Assert.False(reference.IsEmpty);
        Assert.Equal("Microsoft.VisualStudioCode", reference.Id);
        Assert.Equal("winget", reference.Source);
        Assert.Equal("1.0.0", reference.Version);
        Assert.Equal("Microsoft.VisualStudioCode (winget)", reference.DisplayName);
    }

    [Fact]
    public void Reference_None_IsEmpty()
    {
        Assert.True(PackageReference.None.IsEmpty);
    }
}
