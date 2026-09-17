// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using WinGetXAvalonia.Core.Operations;
using WinGetXAvalonia.Core.Providers;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Core;
using WinGetXAvalonia.Services.Operations;
using WinGetXAvalonia.ViewModels;

namespace WinGetXAvalonia.Tests;

public class PackageDetailsViewModelTests
{
    private class FakeProvider : IPackageProvider
    {
        public PackageDetails Details { get; set; } = new();

        public Task<IReadOnlyList<PackageModel>> SearchAsync(string query)
            => Task.FromResult<IReadOnlyList<PackageModel>>([]);

        public Task InstallAsync(string id, IOperationProgress? progress = null) => Task.CompletedTask;

        public Task UpgradeAsync(string id, IOperationProgress? progress = null) => Task.CompletedTask;

        public Task UninstallAsync(string id, IOperationProgress? progress = null) => Task.CompletedTask;

        public Task<IReadOnlyList<InstalledPackage>> GetInstalledAsync()
            => Task.FromResult<IReadOnlyList<InstalledPackage>>([]);

        public Task<IReadOnlyList<UpdatePackage>> GetUpdatesAsync()
            => Task.FromResult<IReadOnlyList<UpdatePackage>>([]);

        public Task<string?> GetHomepageAsync(string id) => Task.FromResult<string?>(null);

        public virtual Task<PackageDetails> GetDetailsAsync(string id) => Task.FromResult(Details);
    }

    private sealed class FakeOperationService : IOperationService
    {
        public List<string> Installed { get; } = new();

        public List<string> Updated { get; } = new();

        public List<string> Removed { get; } = new();

        public Task InstallAsync(string packageId)
        {
            Installed.Add(packageId);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(string packageId)
        {
            Updated.Add(packageId);
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string packageId)
        {
            Removed.Add(packageId);
            return Task.CompletedTask;
        }

        public Task UpgradeAllAsync() => Task.CompletedTask;
    }

    private static PackageDetailsViewModel Create(FakeProvider? provider = null)
        => new(
            provider ?? new FakeProvider(),
            new FakeOperationService(),
            new ClipboardService(),
            new LauncherService());

    private static PackageModel Package(string id = "Git.Git", string name = "Git") => new()
    {
        Id = id,
        Name = name,
        Source = "winget",
        Version = "2.51.0",
        InstallState = PackageInstallState.Installed
    };

    [Fact]
    public async Task Load_ComputesHealth()
    {
        var vm = Create();

        await vm.Load(Package());

        Assert.Equal(100, vm.HealthScore);
        Assert.Equal("100%", vm.HealthScoreLabel);
        Assert.Equal("High", vm.SourceTrustLabel);
        Assert.Equal("Good", vm.UpdateFrequencyLabel);
Assert.Equal("Latest", vm.LatestVersionLabel);
        Assert.Equal("Verified", vm.PublisherLabel);
    }

    [Fact]
    public async Task Load_UpdatePending_LowersScore()
    {
        var vm = Create();

        var package = Package();
        package.Available = "2.52.0";

        await vm.Load(package);

        Assert.Equal(93, vm.HealthScore);
        Assert.Equal("Update available (2.52.0)", vm.LatestVersionLabel);
    }

    [Fact]
    public async Task Load_Null_ResetsHealth()
    {
        var vm = Create();

        await vm.Load(Package());
        await vm.Load(null);

        Assert.Null(vm.SelectedPackage);
        Assert.Equal(0, vm.HealthScore);
        Assert.False(vm.IsLoading);
    }

    [Fact]
    public async Task Load_PopulatesDisplayFromProvider()
    {
        var provider = new FakeProvider
        {
            Details = new PackageDetails
            {
                Publisher = "Git for Windows",
                Description = "Distributed version control",
                Homepage = "https://git-scm.com/",
                License = "MIT",
                ReleaseNotes = "See release notes"
            }
        };

        var vm = Create(provider);

        await vm.Load(Package());

        Assert.Equal("Git for Windows", vm.PublisherDisplay);
        Assert.Equal("Distributed version control", vm.DescriptionDisplay);
        Assert.Equal("https://git-scm.com/", vm.HomepageDisplay);
        Assert.Equal("MIT", vm.LicenseDisplay);
        Assert.Equal("See release notes", vm.ReleaseNotesDisplay);
        Assert.True(vm.HasHomepage);
        Assert.False(vm.IsLoading);
    }

    [Fact]
    public async Task Load_DescriptionFallsBackToPackage()
    {
        var vm = Create();

        var package = Package();
        package.Description = "From card";

        await vm.Load(package);

        Assert.Equal("From card", vm.DescriptionDisplay);
    }

    [Fact]
    public async Task Load_PopulatesRawProperties()
    {
        var provider = new FakeProvider
        {
            Details = new PackageDetails
            {
                Name = "Git",
                Publisher = "Git for Windows",
                Description = "Distributed version control",
                Version = "2.51.0",
                LatestVersion = "2.52.0",
                Homepage = "https://git-scm.com/",
                License = "MIT",
                Source = "winget"
            }
        };

        var vm = Create(provider);

        await vm.Load(Package());

        Assert.Equal("Git", vm.Name);
        Assert.Equal("Git for Windows", vm.Publisher);
        Assert.Equal("Distributed version control", vm.Description);
        Assert.Equal("2.51.0", vm.InstalledVersion);
        Assert.Equal("2.52.0", vm.LatestVersion);
        Assert.Equal("https://git-scm.com/", vm.Homepage);
        Assert.Equal("MIT", vm.License);
        Assert.Equal("winget", vm.Source);
    }

    [Fact]
    public async Task Load_RawVersionsFallBackToPackage()
    {
        var vm = Create();

        var package = Package();
        package.Available = "2.52.0";

        await vm.Load(package);

        Assert.Equal("2.51.0", vm.InstalledVersion);
        Assert.Equal("2.52.0", vm.LatestVersion);
        Assert.Equal("winget", vm.Source);
    }

    [Fact]
    public async Task Load_Null_ResetsRawProperties()
    {
        var vm = Create();

        await vm.Load(Package());
        await vm.Load(null);

        Assert.Equal("Not available", vm.Name);
        Assert.Equal("Not available", vm.Description);
        Assert.Equal("", vm.Homepage);
        Assert.Equal("Not available", vm.Publisher);
        Assert.Equal("Not available", vm.License);
        Assert.Equal("Not available", vm.Source);
    }

    [Fact]
    public async Task Load_ProviderThrows_FallsBackToPackage()
    {
        var vm = Create(new ThrowingProvider());

        await vm.Load(Package());

        Assert.Equal("Git", vm.Name);
        Assert.Equal("2.51.0", vm.InstalledVersion);
        Assert.Equal("winget", vm.Source);
        Assert.Equal("Not available", vm.Publisher);
        Assert.Equal("", vm.Homepage);
        Assert.False(vm.IsLoading);
    }

    [Fact]
    public async Task Load_ProviderThrows_UsesFallbackText()
    {
        var provider = new ThrowingProvider();

        var vm = Create(provider);

        await vm.Load(Package());

        Assert.Equal("Not available", vm.PublisherDisplay);
        Assert.Equal("Not available", vm.LicenseDisplay);
        Assert.Equal("Not available", vm.ReleaseNotesDisplay);
        Assert.False(vm.HasHomepage);
        Assert.False(vm.IsLoading);
    }

    [Fact]
    public async Task Install_InvokesOperationService()
    {
        var operations = new FakeOperationService();
        var vm = new PackageDetailsViewModel(
            new FakeProvider(),
            operations,
            new ClipboardService(),
            new LauncherService());

        await vm.Load(Package());

        await vm.InstallCommand.ExecuteAsync(null);

        Assert.Contains("Git.Git", operations.Installed);
    }

    [Fact]
    public async Task Update_InvokesOperationService()
    {
        var operations = new FakeOperationService();
        var vm = new PackageDetailsViewModel(
            new FakeProvider(),
            operations,
            new ClipboardService(),
            new LauncherService());

        await vm.Load(Package());

        await vm.UpdateCommand.ExecuteAsync(null);

        Assert.Contains("Git.Git", operations.Updated);
    }

    [Fact]
    public async Task Remove_InvokesOperationService()
    {
        var operations = new FakeOperationService();
        var vm = new PackageDetailsViewModel(
            new FakeProvider(),
            operations,
            new ClipboardService(),
            new LauncherService());

        await vm.Load(Package());

        await vm.RemoveCommand.ExecuteAsync(null);

        Assert.Contains("Git.Git", operations.Removed);
    }

    [Fact]
    public async Task Commands_NoSelection_DoNothing()
    {
        var operations = new FakeOperationService();
        var vm = new PackageDetailsViewModel(
            new FakeProvider(),
            operations,
            new ClipboardService(),
            new LauncherService());

        await vm.Load(null);

        await vm.InstallCommand.ExecuteAsync(null);
        await vm.UpdateCommand.ExecuteAsync(null);
        await vm.RemoveCommand.ExecuteAsync(null);

        Assert.Empty(operations.Installed);
        Assert.Empty(operations.Updated);
        Assert.Empty(operations.Removed);
    }

    private sealed class ThrowingProvider : FakeProvider
    {
        public override Task<PackageDetails> GetDetailsAsync(string id)
            => throw new InvalidOperationException("boom");
    }
}
