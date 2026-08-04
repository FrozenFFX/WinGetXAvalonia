// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using WinGetXAvalonia.Core.Providers;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Core;
using WinGetXAvalonia.Services.Operations;
using WinGetXAvalonia.ViewModels;

namespace WinGetXAvalonia.Tests;

public class BrowseViewModelTests
{
    private sealed class FakeProvider : IPackageProvider
    {
        public int SearchCalls { get; private set; }

        public Dictionary<string, List<PackageModel>> SearchResults { get; } =
            new(StringComparer.OrdinalIgnoreCase);

        public List<InstalledPackage> Installed { get; } = new();

        public List<UpdatePackage> Updates { get; } = new();

        public string? Homepage { get; set; }

        public PackageDetails Details { get; set; } = new();

        public Task<IReadOnlyList<PackageModel>> SearchAsync(string query)
        {
            SearchCalls++;
            SearchResults.TryGetValue(query, out var results);
            return Task.FromResult<IReadOnlyList<PackageModel>>(results ?? []);
        }

        public Task<IReadOnlyList<InstalledPackage>> GetInstalledAsync()
            => Task.FromResult<IReadOnlyList<InstalledPackage>>(Installed);

        public Task<IReadOnlyList<UpdatePackage>> GetUpdatesAsync()
            => Task.FromResult<IReadOnlyList<UpdatePackage>>(Updates);

        public Task InstallAsync(string id) => Task.CompletedTask;

        public Task UpgradeAsync(string id) => Task.CompletedTask;

        public Task UninstallAsync(string id) => Task.CompletedTask;

        public Task<string?> GetHomepageAsync(string id)
            => Task.FromResult(Homepage);

        public Task<PackageDetails> GetDetailsAsync(string id)
            => Task.FromResult(Details);
    }

    private sealed class FakeFeaturedCatalog : IFeaturedCatalog
    {
        public IReadOnlyList<FeaturedItem> Items { get; init; } = [];

        public Task<IReadOnlyList<FeaturedItem>> GetAsync() => Task.FromResult(Items);
    }

    private sealed class FakeOperationService : IOperationService
    {
        public List<string> Installed { get; } = new();

        public List<string> Updated { get; } = new();

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

        public Task RemoveAsync(string packageId) => Task.CompletedTask;

        public Task UpgradeAllAsync() => Task.CompletedTask;
    }

    private static BrowseViewModel Create(
        FakeProvider provider,
        IFeaturedCatalog catalog,
        FakeOperationService? operations = null)
        => new(
            provider,
            operations ?? new FakeOperationService(),
            new PackageSearchCache(),
            catalog,
            new ClipboardService(),
            new LauncherService());

    [Fact]
    public async Task InitializeAsync_PopulatesFeaturedFromJsonIds()
    {
        var provider = new FakeProvider();
        provider.SearchResults["Git.Git"] = new() { new() { Id = "Git.Git", Name = "Git" } };
        provider.SearchResults["Microsoft.VisualStudioCode"] = new()
        {
            new() { Id = "Microsoft.VisualStudioCode", Name = "Visual Studio Code" }
        };

        var catalog = new FakeFeaturedCatalog
        {
            Items = new[]
            {
                new FeaturedItem { Title = "Git", Id = "Git.Git" },
                new FeaturedItem { Title = "Visual Studio Code", Id = "Microsoft.VisualStudioCode" }
            }
        };

        var vm = Create(provider, catalog);
        await vm.InitializeAsync();

        Assert.Equal(2, vm.Featured.Count);
        Assert.Contains(vm.Featured, p => p.Name == "Git");
        Assert.Contains(vm.Featured, p => p.Name == "Visual Studio Code");
    }

    [Fact]
    public async Task InitializeAsync_SkipsMissingPackages()
    {
        var provider = new FakeProvider();
        var catalog = new FakeFeaturedCatalog
        {
            Items = new[] { new FeaturedItem { Title = "Ghost", Id = "No.Such" } }
        };

        var vm = Create(provider, catalog);
        await vm.InitializeAsync();

        Assert.Empty(vm.Featured);
    }

    [Fact]
    public async Task Search_PopulatesResults_AndReusesCache()
    {
        var provider = new FakeProvider();
        provider.SearchResults["git"] = new() { new() { Id = "Git.Git", Name = "Git" } };

        var vm = Create(provider, new FakeFeaturedCatalog());

        vm.SearchText = "git";
        await vm.SearchCommand.ExecuteAsync(null);

        Assert.Single(vm.Results);
        Assert.Equal(1, provider.SearchCalls);

        vm.SearchText = "git";
        await vm.SearchCommand.ExecuteAsync(null);

        Assert.Single(vm.Results);
        Assert.Equal(1, provider.SearchCalls);
    }

    [Fact]
    public async Task ResultsSummary_ReflectsResultCount()
    {
        var provider = new FakeProvider();
        provider.SearchResults["git"] = new() { new() { Id = "Git.Git", Name = "Git" } };

        var vm = Create(provider, new FakeFeaturedCatalog());

        Assert.Equal("No packages found", vm.ResultsSummary);

        vm.SearchText = "git";
        await vm.SearchCommand.ExecuteAsync(null);

        Assert.Equal("1 package found", vm.ResultsSummary);

        provider.SearchResults["all"] = new()
        {
            new() { Id = "Git.Git", Name = "Git" },
            new() { Id = "Microsoft.VisualStudioCode", Name = "VS Code" }
        };

        vm.SearchText = "all";
        await vm.SearchCommand.ExecuteAsync(null);

        Assert.Equal("2 packages found", vm.ResultsSummary);
    }

    [Fact]
    public async Task ClearSearch_ResetsSearchAndResults()
    {
        var provider = new FakeProvider();
        provider.SearchResults["git"] = new() { new() { Id = "Git.Git", Name = "Git" } };

        var vm = Create(provider, new FakeFeaturedCatalog());

        vm.SearchText = "git";
        await vm.SearchCommand.ExecuteAsync(null);

        Assert.Single(vm.Results);

        vm.ClearSearchCommand.Execute(null);

        Assert.Empty(vm.Results);
        Assert.Equal("", vm.SearchText);
        Assert.False(vm.IsEmpty);
        Assert.False(vm.HasError);
        Assert.Equal("No packages found", vm.ResultsSummary);
    }

    [Fact]
    public void SelectingSuggestion_SetsSearchText()
    {
        var vm = Create(new FakeProvider(), new FakeFeaturedCatalog());

        vm.SelectedSuggestion = "git";

        Assert.Equal("git", vm.SearchText);
    }

    [Fact]
    public async Task SelectingPackage_LoadsDetails()
    {
        var provider = new FakeProvider
        {
            Details = new PackageDetails
            {
                Publisher = "Git for Windows",
                Description = "Distributed version control",
                Homepage = "https://git-scm.com/",
                License = "MIT"
            }
        };

        var vm = Create(provider, new FakeFeaturedCatalog());

        var package = new PackageModel { Id = "Git.Git", Name = "Git" };

        vm.SelectedPackage = package;

        await Task.Delay(1);

        Assert.Same(package, vm.SelectedDetails.SelectedPackage);
        Assert.Equal("Git for Windows", vm.SelectedDetails.PublisherDisplay);
        Assert.Equal("Distributed version control", vm.SelectedDetails.DescriptionDisplay);
        Assert.Equal("MIT", vm.SelectedDetails.LicenseDisplay);
        Assert.True(vm.SelectedDetails.HasHomepage);
        Assert.Equal("https://git-scm.com/", vm.SelectedDetails.HomepageDisplay);
        Assert.False(vm.SelectedDetails.IsLoading);
    }

    [Fact]
    public void SelectingPackage_TogglesSelectedHighlight()
    {
        var vm = Create(new FakeProvider(), new FakeFeaturedCatalog());

        var first = new PackageModel { Id = "A", Name = "A" };
        var second = new PackageModel { Id = "B", Name = "B" };

        vm.SelectedPackage = first;

        Assert.True(first.Selected);
        Assert.False(second.Selected);

        vm.SelectedPackage = second;

        Assert.False(first.Selected);
        Assert.True(second.Selected);

        vm.SelectedPackage = null;

        Assert.False(second.Selected);
    }

    [Fact]
    public async Task SelectingPackage_MissingDetails_UsesFallbackText()
    {
        var provider = new FakeProvider();
        var vm = Create(provider, new FakeFeaturedCatalog());

        vm.SelectedPackage = new PackageModel { Id = "Git.Git", Name = "Git" };

        await Task.Delay(1);

        Assert.Equal("Not available", vm.SelectedDetails.PublisherDisplay);
        Assert.Equal("Not available", vm.SelectedDetails.LicenseDisplay);
        Assert.Equal("Not available", vm.SelectedDetails.ReleaseNotesDisplay);
        Assert.False(vm.SelectedDetails.HasHomepage);
    }

    [Fact]
    public async Task SelectingPackage_DetailsFallBackToSearchDescription()
    {
        var provider = new FakeProvider();
        var vm = Create(provider, new FakeFeaturedCatalog());

        var package = new PackageModel { Id = "Git.Git", Name = "Git", Description = "From card" };

        vm.SelectedPackage = package;

        await Task.Delay(1);

        Assert.Equal("From card", vm.SelectedDetails.DescriptionDisplay);
    }

    [Fact]
    public async Task Search_EnrichesResults_WithInstalledAndAvailable()
    {
        var provider = new FakeProvider();
        provider.SearchResults["git"] = new() { new() { Id = "Git.Git", Name = "Git" } };
        provider.Installed.Add(new InstalledPackage { Id = "Git.Git", Version = "2.51.0" });
        provider.Updates.Add(new UpdatePackage { Id = "Git.Git", Available = "2.52.0" });

        var vm = Create(provider, new FakeFeaturedCatalog());

        vm.SearchText = "git";
        await vm.SearchCommand.ExecuteAsync(null);

        var package = Assert.Single(vm.Results);
        Assert.Equal("2.51.0", package.Version);
        Assert.Equal("2.52.0", package.Available);
    }

    [Fact]
    public async Task Search_NoResults_SetsEmptyState()
    {
        var provider = new FakeProvider();
        var vm = Create(provider, new FakeFeaturedCatalog());

        vm.SearchText = "nothing";
        await vm.SearchCommand.ExecuteAsync(null);

        Assert.Empty(vm.Results);
        Assert.True(vm.IsEmpty);
    }

    [Fact]
    public async Task SelectCategory_SearchesUsingCategoryQuery()
    {
        var provider = new FakeProvider();
        provider.SearchResults["git vscode docker"] = new()
        {
            new() { Id = "Git.Git", Name = "Git" },
            new() { Id = "Microsoft.VisualStudioCode", Name = "Visual Studio Code" }
        };

        var vm = Create(provider, new FakeFeaturedCatalog());
        var category = vm.Categories[0];

        await vm.SelectCategoryCommand.ExecuteAsync(category);

        Assert.Equal(2, vm.Results.Count);
        Assert.Equal(category.Query, vm.SearchText);
    }

    [Fact]
    public async Task Install_InvokesOperationService()
    {
        var provider = new FakeProvider();
        provider.SearchResults["git"] = new() { new() { Id = "Git.Git", Name = "Git" } };
        var operations = new FakeOperationService();

        var vm = Create(provider, new FakeFeaturedCatalog(), operations);

        vm.SearchText = "git";
        await vm.SearchCommand.ExecuteAsync(null);

        await vm.InstallCommand.ExecuteAsync(vm.Results[0]);

        Assert.Contains("Git.Git", operations.Installed);
    }

    [Fact]
    public async Task Update_InvokesOperationService()
    {
        var provider = new FakeProvider();
        provider.SearchResults["git"] = new() { new() { Id = "Git.Git", Name = "Git" } };
        var operations = new FakeOperationService();

        var vm = Create(provider, new FakeFeaturedCatalog(), operations);

        vm.SearchText = "git";
        await vm.SearchCommand.ExecuteAsync(null);

        await vm.UpdateCommand.ExecuteAsync(vm.Results[0]);

        Assert.Contains("Git.Git", operations.Updated);
    }

    [Fact]
    public async Task Search_EnrichesResults_WithInstallState()
    {
        var provider = new FakeProvider();
        provider.SearchResults["git"] = new()
        {
            new() { Id = "Git.Git", Name = "Git" },
            new() { Id = "Microsoft.VisualStudioCode", Name = "VS Code" }
        };
        provider.Installed.Add(new InstalledPackage { Id = "Git.Git", Version = "2.51.0" });
        provider.Updates.Add(new UpdatePackage { Id = "Git.Git", Available = "2.52.0" });

        var vm = Create(provider, new FakeFeaturedCatalog());

        vm.SearchText = "git";
        await vm.SearchCommand.ExecuteAsync(null);

        var git = vm.Results.Single(p => p.Id == "Git.Git");
        var code = vm.Results.Single(p => p.Id == "Microsoft.VisualStudioCode");

        Assert.Equal(PackageInstallState.Installed, git.InstallState);
        Assert.Equal("2.51.0", git.InstalledVersionDisplay);
        Assert.Equal("2.52.0", git.AvailableVersionDisplay);
        Assert.Equal("Update", git.InstallButtonText);
        Assert.True(git.CanInstall);

        Assert.Equal(PackageInstallState.NotInstalled, code.InstallState);
        Assert.Equal("-", code.InstalledVersionDisplay);
        Assert.Equal("-", code.AvailableVersionDisplay);
        Assert.Equal("Install", code.InstallButtonText);
        Assert.True(code.CanInstall);
    }

    [Fact]
    public async Task InstallOrUpdate_Installs_AndMarksInstalled()
    {
        var provider = new FakeProvider();
        var operations = new FakeOperationService();

        var vm = Create(provider, new FakeFeaturedCatalog(), operations);

        var package = new PackageModel { Id = "Git.Git", Name = "Git" };

        await vm.InstallOrUpdateCommand.ExecuteAsync(package);

        Assert.Contains("Git.Git", operations.Installed);
        Assert.Equal(PackageInstallState.Installed, package.InstallState);
        Assert.Equal("Installed", package.InstallButtonText);
        Assert.False(package.CanInstall);
    }

    [Fact]
    public async Task InstallOrUpdate_Installing_ShowsProgressText()
    {
        var provider = new FakeProvider();
        var operations = new FakeOperationService();

        var vm = Create(provider, new FakeFeaturedCatalog(), operations);

        var package = new PackageModel { Id = "Git.Git", Name = "Git" };
        package.InstallState = PackageInstallState.Installing;

        Assert.True(package.IsBusy);
        Assert.Equal("Installing...", package.InstallButtonText);
        Assert.False(package.CanInstall);
    }

    [Fact]
    public async Task InstallOrUpdate_Updates_WhenUpdateAvailable()
    {
        var provider = new FakeProvider();
        var operations = new FakeOperationService();

        var vm = Create(provider, new FakeFeaturedCatalog(), operations);

        var package = new PackageModel
        {
            Id = "Git.Git",
            Name = "Git",
            Version = "2.51.0",
            Available = "2.52.0",
            InstallState = PackageInstallState.Installed
        };

        await vm.InstallOrUpdateCommand.ExecuteAsync(package);

        Assert.Contains("Git.Git", operations.Updated);
        Assert.Equal("", package.Available);
        Assert.Equal(PackageInstallState.Installed, package.InstallState);
    }
}
