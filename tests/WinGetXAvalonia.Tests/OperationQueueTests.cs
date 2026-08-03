// ==========================================================
// WinGetX - https://github.com/FrozenFFX
// ==========================================================
using WinGetXAvalonia.Core.Operations;
using WinGetXAvalonia.Core.Providers;
using WinGetXAvalonia.Interfaces;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Core;

namespace WinGetXAvalonia.Tests;

// kaneki
public class OperationQueueTests
{
    // kaneki
    private sealed class FakeProvider : IPackageProvider
    {
        public TaskCompletionSource? Blocking { get; set; }

        public bool Throws { get; set; }

        public List<string> Installed { get; } = new();

        public List<string> Upgraded { get; } = new();

        public List<string> Uninstalled { get; } = new();

        public Task<IReadOnlyList<PackageModel>> SearchAsync(string query)
            => Task.FromResult<IReadOnlyList<PackageModel>>(Array.Empty<PackageModel>());

        public Task<IReadOnlyList<InstalledPackage>> GetInstalledAsync()
            => Task.FromResult<IReadOnlyList<InstalledPackage>>(Array.Empty<InstalledPackage>());

        public Task<IReadOnlyList<UpdatePackage>> GetUpdatesAsync()
            => Task.FromResult<IReadOnlyList<UpdatePackage>>(Array.Empty<UpdatePackage>());

        public async Task InstallAsync(string id)
        {
            Installed.Add(id);

            if (Throws)
                throw new InvalidOperationException("boom");

            if (Blocking is not null)
                await Blocking.Task;
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

    private static async Task WaitUntilAsync(Func<bool> condition, int timeoutMs = 5000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);

        while (!condition())
        {
            if (DateTime.UtcNow > deadline)
                throw new TimeoutException("Timed out waiting for condition");

            await Task.Delay(20);
        }
    }

    [Fact]
    public void PackageOperation_DefaultsToQueued()
    {
        var operation = new PackageOperation("Microsoft.VSCode", PackageOperationType.Install);

        Assert.NotEqual(Guid.Empty, operation.Id);
        Assert.Equal("Microsoft.VSCode", operation.PackageId);
        Assert.Equal(PackageOperationType.Install, operation.Type);
        Assert.Equal(PackageOperationStatus.Queued, operation.Status);
        Assert.Equal(0, operation.Progress);
        Assert.Null(operation.FinishedAt);
    }

    [Fact]
    public async Task EnqueueAsync_RunsInstall_AndCompletes()
    {
        var provider = new FakeProvider();
        var queue = new OperationQueue(provider, new LoggerService());
        var operation = new PackageOperation("Microsoft.VSCode", PackageOperationType.Install);

        await queue.EnqueueAsync(operation);
        await WaitUntilAsync(() => operation.Status == PackageOperationStatus.Completed);

        Assert.Contains("Microsoft.VSCode", provider.Installed);
        Assert.Equal(PackageOperationStatus.Completed, operation.Status);
        Assert.Equal(100, operation.Progress);
        Assert.NotNull(operation.FinishedAt);
        Assert.Single(queue.Operations);
    }

    [Fact]
    public async Task EnqueueAsync_RunsUpdate_ThroughUpgrade()
    {
        var provider = new FakeProvider();
        var queue = new OperationQueue(provider, new LoggerService());
        var operation = new PackageOperation("Microsoft.VSCode", PackageOperationType.Update);

        await queue.EnqueueAsync(operation);
        await WaitUntilAsync(() => operation.Status == PackageOperationStatus.Completed);

        Assert.Contains("Microsoft.VSCode", provider.Upgraded);
    }

    [Fact]
    public async Task EnqueueAsync_SetsFailed_OnProviderError()
    {
        var provider = new FakeProvider { Throws = true };
        var queue = new OperationQueue(provider, new LoggerService());
        var operation = new PackageOperation("Microsoft.VSCode", PackageOperationType.Install);

        await queue.EnqueueAsync(operation);
        await WaitUntilAsync(() => operation.Status == PackageOperationStatus.Failed);

        Assert.Equal(PackageOperationStatus.Failed, operation.Status);
        Assert.NotNull(operation.FinishedAt);
    }

    [Fact]
    public async Task CancelAsync_CancelsQueuedOperation_NotRunning()
    {
        var provider = new FakeProvider { Blocking = new TaskCompletionSource() };
        var queue = new OperationQueue(provider, new LoggerService());
        var first = new PackageOperation("first", PackageOperationType.Install);
        var second = new PackageOperation("second", PackageOperationType.Install);

        await queue.EnqueueAsync(first);
        await WaitUntilAsync(() => first.Status == PackageOperationStatus.Running);

        await queue.EnqueueAsync(second);
        await queue.CancelAsync(second.Id);

        Assert.Equal(PackageOperationStatus.Cancelled, second.Status);
        Assert.NotNull(second.FinishedAt);

        provider.Blocking!.SetResult();
        await WaitUntilAsync(() => first.Status == PackageOperationStatus.Completed);

        Assert.Equal(PackageOperationStatus.Cancelled, second.Status);
        Assert.DoesNotContain("second", provider.Installed);
    }
}
// kaneki
