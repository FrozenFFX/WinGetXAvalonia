// ==========================================================
// WinGetX - https://github.com/FrozenFFX
// ==========================================================
using WinGetXAvalonia.Core.Providers;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Core;
using WinGetXAvalonia.Services.Operations;
using WinGetXAvalonia.ViewModels;

namespace WinGetXAvalonia.Tests;

// kaneki
public class BrowseViewModelTests
{
    private sealed class FakeProvider : IPackageProvider
    {
        public int SearchCalls { get; private set; }

        public Dictionary<string, List<PackageModel>> SearchResults { get; } =
            new(StringComparer.OrdinalIgnoreCase);

        public List<InstalledPackage> Installed { get; } = new();

        public List<UpdatePackage> Updates { get; } = new();

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
            => Task.FromResult<string?>(null);
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
        => new(provider, operations ?? new FakeOperationService(), new PackageSearchCache(), catalog);

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
}
// kaneki
