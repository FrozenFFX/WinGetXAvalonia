// ==========================================================
// WinGetX - https://github.com/FrozenFFX
// ==========================================================
using WinGetXAvalonia.Core.Providers;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Operations;
using WinGetXAvalonia.ViewModels;

namespace WinGetXAvalonia.Tests;

// kaneki
public class InstalledViewModelTests
{
    private sealed class FakeProvider : IPackageProvider
    {
        public List<InstalledPackage> Installed { get; } = new();

        public Task<IReadOnlyList<InstalledPackage>> GetInstalledAsync()
            => Task.FromResult<IReadOnlyList<InstalledPackage>>(Installed);

        public Task<IReadOnlyList<PackageModel>> SearchAsync(string query)
            => Task.FromResult<IReadOnlyList<PackageModel>>(Array.Empty<PackageModel>());

        public Task<IReadOnlyList<UpdatePackage>> GetUpdatesAsync()
            => Task.FromResult<IReadOnlyList<UpdatePackage>>(Array.Empty<UpdatePackage>());

        public Task InstallAsync(string id) => Task.CompletedTask;

        public Task UpgradeAsync(string id) => Task.CompletedTask;

        public Task UninstallAsync(string id) => Task.CompletedTask;

        public Task<string?> GetHomepageAsync(string id)
            => Task.FromResult<string?>(null);
    }

    private sealed class FakeOperationService : IOperationService
    {
        public List<string> Removed { get; } = new();

        public List<string> Updated { get; } = new();

        public Task InstallAsync(string packageId) => Task.CompletedTask;

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

    private static (InstalledViewModel vm, FakeProvider provider, FakeOperationService operations) Create(
        params string[] ids)
    {
        var provider = new FakeProvider();

        foreach (string id in ids)
            provider.Installed.Add(new InstalledPackage { Id = id, Name = id, Version = "1.0.0" });

        var operations = new FakeOperationService();

        var vm = new InstalledViewModel(provider, operations);

        return (vm, provider, operations);
    }

    [Fact]
    public async Task Refresh_PopulatesPackages()
    {
        var (vm, _, _) = Create("Git.Git", "Microsoft.VisualStudioCode", "Microsoft.PowerToys");

        await vm.RefreshCommand.ExecuteAsync(null);

        Assert.Equal(3, vm.Packages.Count);
        Assert.Equal(3, vm.PackageCount);
    }

    [Fact]
    public async Task Selected_Toggle_UpdatesSelectedCount()
    {
        var (vm, _, _) = Create("a", "b", "c");
        await vm.RefreshCommand.ExecuteAsync(null);

        vm.Packages[0].Selected = true;
        vm.Packages[1].Selected = true;

        Assert.Equal(2, vm.SelectedCount);

        vm.Packages[1].Selected = false;

        Assert.Equal(1, vm.SelectedCount);
    }

    [Fact]
    public async Task RemoveSelected_RemovesOnlySelected()
    {
        var (vm, _, operations) = Create("a", "b", "c");
        await vm.RefreshCommand.ExecuteAsync(null);

        vm.Packages[0].Selected = true;
        vm.Packages[2].Selected = true;

        await vm.RemoveSelectedCommand.ExecuteAsync(null);

        Assert.Single(vm.Packages);
        Assert.Equal("b", vm.Packages[0].Id);
        Assert.Equal(0, vm.SelectedCount);
        Assert.Equal(new[] { "a", "c" }, operations.Removed.OrderBy(x => x));
    }

    [Fact]
    public async Task UpdateSelected_UpdatesEach_AndRefreshes()
    {
        var (vm, _, operations) = Create("a", "b", "c");
        await vm.RefreshCommand.ExecuteAsync(null);

        vm.Packages[0].Selected = true;
        vm.Packages[2].Selected = true;

        await vm.UpdateSelectedCommand.ExecuteAsync(null);

        Assert.Equal(new[] { "a", "c" }, operations.Updated.OrderBy(x => x));
        Assert.Equal(3, vm.Packages.Count);
    }
}
// kaneki
