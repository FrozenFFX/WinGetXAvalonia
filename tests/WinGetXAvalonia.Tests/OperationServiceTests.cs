// ==========================================================
// WinGetX - https://github.com/FrozenFFX
// ==========================================================
using WinGetXAvalonia.Core.Providers;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Core;
using WinGetXAvalonia.Services.History;
using WinGetXAvalonia.Services.Operations;

namespace WinGetXAvalonia.Tests;

// kaneki
public class OperationServiceTests
{
    // kaneki
    private sealed class FakeProvider : IPackageProvider
    {
        public bool Throws { get; set; }

        public List<string> Installed { get; } = new();

        public List<string> Upgraded { get; } = new();

        public List<string> Uninstalled { get; } = new();

        public List<UpdatePackage> Updates { get; set; } = new();

        public Task<IReadOnlyList<PackageModel>> SearchAsync(string query)
            => Task.FromResult<IReadOnlyList<PackageModel>>(Array.Empty<PackageModel>());

        public Task<IReadOnlyList<InstalledPackage>> GetInstalledAsync()
            => Task.FromResult<IReadOnlyList<InstalledPackage>>(Array.Empty<InstalledPackage>());

        public Task<IReadOnlyList<UpdatePackage>> GetUpdatesAsync()
            => Task.FromResult<IReadOnlyList<UpdatePackage>>(Updates.ToArray());

        public async Task InstallAsync(string id)
        {
            Installed.Add(id);

            if (Throws)
                throw new InvalidOperationException("boom");
        }

        public Task UpgradeAsync(string id)
        {
            Upgraded.Add(id);
            return Task.CompletedTask;
        }

        public Task UninstallAsync(string id)
        {
            Uninstalled.Add(id);
            return Task.CompletedTask;
        }

        public Task<string?> GetHomepageAsync(string id)
            => Task.FromResult<string?>(null);
    }

    private static string CreateTempFile()
    {
        var dir = Path.Combine(
            Path.GetTempPath(),
            "WinGetXAvalonia.Tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, "history.json");
    }

    private static (OperationService Service, FakeProvider Provider, HistoryService History) Create()
    {
        var provider = new FakeProvider();
        var history = new HistoryService(CreateTempFile());
        var service = new OperationService(provider, history, new LoggerService());
        return (service, provider, history);
    }

    private static async Task<List<HistoryEntry>> GetEntriesAsync(HistoryService history)
        => (await history.GetAllAsync()).ToList();

    [Fact]
    public async Task InstallAsync_CallsProvider_AndRecordsHistory()
    {
        var (service, provider, history) = Create();

        await service.InstallAsync("Git.Git");

        Assert.Contains("Git.Git", provider.Installed);

        var entry = Assert.Single(await GetEntriesAsync(history));
        Assert.Equal("Install", entry.Operation);
        Assert.Equal("Git.Git", entry.PackageId);
        Assert.True(entry.Success);
    }

    [Fact]
    public async Task UpdateAsync_CallsUpgrade_AndRecordsHistory()
    {
        var (service, provider, history) = Create();

        await service.UpdateAsync("Git.Git");

        Assert.Contains("Git.Git", provider.Upgraded);

        var entry = Assert.Single(await GetEntriesAsync(history));
        Assert.Equal("Update", entry.Operation);
        Assert.True(entry.Success);
    }

    [Fact]
    public async Task RemoveAsync_CallsUninstall_AndRecordsHistory()
    {
        var (service, provider, history) = Create();

        await service.RemoveAsync("Git.Git");

        Assert.Contains("Git.Git", provider.Uninstalled);

        var entry = Assert.Single(await GetEntriesAsync(history));
        Assert.Equal("Remove", entry.Operation);
        Assert.True(entry.Success);
    }

    [Fact]
    public async Task Operation_Failed_RecordsFailure_WithoutThrowing()
    {
        var (service, provider, history) = Create();
        provider.Throws = true;

        await service.InstallAsync("Git.Git");

        var entry = Assert.Single(await GetEntriesAsync(history));
        Assert.False(entry.Success);
        Assert.Equal("boom", entry.Message);
    }

    [Fact]
    public async Task BlankPackageId_SkipsProviderAndHistory()
    {
        var (service, provider, history) = Create();

        await service.InstallAsync("   ");

        Assert.Empty(provider.Installed);
        Assert.Empty(await GetEntriesAsync(history));
    }

    [Fact]
    public async Task UpgradeAllAsync_UpgradesEveryUpdate_AndRecordsPerPackage()
    {
        var (service, provider, history) = Create();
        provider.Updates =
        [
            new UpdatePackage { Id = "Git.Git" },
            new UpdatePackage { Id = "Microsoft.VSCode" }
        ];

        await service.UpgradeAllAsync();

        Assert.Equal(2, provider.Upgraded.Count);
        Assert.Contains("Git.Git", provider.Upgraded);
        Assert.Contains("Microsoft.VSCode", provider.Upgraded);

        var entries = await GetEntriesAsync(history);
        Assert.Equal(2, entries.Count);
        Assert.All(entries, e => Assert.Equal("Update", e.Operation));
    }
}
// kaneki
