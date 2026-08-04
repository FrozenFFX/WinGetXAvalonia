using WinGetXAvalonia.Core.Events;
using WinGetXAvalonia.Core.Providers;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Core;
using WinGetXAvalonia.Services.History;
using WinGetXAvalonia.Services.Notifications;
using WinGetXAvalonia.Services.Operations;

namespace WinGetXAvalonia.Tests;

public class OperationServiceTests
{
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

        public Task<PackageDetails> GetDetailsAsync(string id)
            => Task.FromResult(new PackageDetails());
    }

    private sealed class FakeOperationQueue : IOperationQueue
    {
        public List<OperationItem> Items { get; } = new();

        public List<OperationItem> Enqueued { get; } = new();

        public IReadOnlyList<OperationItem> Operations => Items;

        public event EventHandler? QueueChanged;

        public async Task EnqueueAsync(OperationItem item, Func<Task> action)
        {
            Items.Add(item);
            Enqueued.Add(item);
            item.Status = OperationStatus.Running;

            try
            {
                await action();
                item.Progress = 100;
                item.Status = OperationStatus.Completed;
            }
            catch (Exception ex)
            {
                item.Status = OperationStatus.Failed;
                item.Message = ex.Message;
            }

            QueueChanged?.Invoke(this, EventArgs.Empty);
        }
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

    private static (OperationService Service, FakeProvider Provider, HistoryService History, FakeEventBus Events, FakeOperationQueue Queue, NotificationService Notifications) Create()
    {
        var provider = new FakeProvider();
        var history = new HistoryService(CreateTempFile());
        var events = new FakeEventBus();
        var queue = new FakeOperationQueue();
        var notifications = new NotificationService();
        var service = new OperationService(provider, history, new LoggerService(), events, queue, notifications);
        return (service, provider, history, events, queue, notifications);
    }

    private static async Task<List<HistoryEntry>> GetEntriesAsync(HistoryService history)
        => (await history.GetAllAsync()).ToList();

    [Fact]
    public async Task InstallAsync_CallsProvider_AndRecordsHistory()
    {
        var (service, provider, history, eventBus, queue, notifications) = Create();

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
        var (service, provider, history, eventBus, queue, notifications) = Create();

        await service.UpdateAsync("Git.Git");

        Assert.Contains("Git.Git", provider.Upgraded);

        var entry = Assert.Single(await GetEntriesAsync(history));
        Assert.Equal("Update", entry.Operation);
        Assert.True(entry.Success);
    }

    [Fact]
    public async Task RemoveAsync_CallsUninstall_AndRecordsHistory()
    {
        var (service, provider, history, eventBus, queue, notifications) = Create();

        await service.RemoveAsync("Git.Git");

        Assert.Contains("Git.Git", provider.Uninstalled);

        var entry = Assert.Single(await GetEntriesAsync(history));
        Assert.Equal("Remove", entry.Operation);
        Assert.True(entry.Success);
    }

    [Fact]
    public async Task Operation_Failed_RecordsFailure_WithoutThrowing()
    {
        var (service, provider, history, eventBus, queue, notifications) = Create();
        provider.Throws = true;

        await service.InstallAsync("Git.Git");

        var entry = Assert.Single(await GetEntriesAsync(history));
        Assert.False(entry.Success);
        Assert.Equal("boom", entry.Message);
    }

    [Fact]
    public async Task BlankPackageId_SkipsProviderAndHistory()
    {
        var (service, provider, history, eventBus, queue, notifications) = Create();

        await service.InstallAsync("   ");

        Assert.Empty(provider.Installed);
        Assert.Empty(await GetEntriesAsync(history));
    }

    [Fact]
    public async Task UpgradeAllAsync_UpgradesEveryUpdate_AndRecordsPerPackage()
    {
        var (service, provider, history, eventBus, queue, notifications) = Create();
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

    [Theory]
    [InlineData("Install", PackageOperation.Installed)]
    [InlineData("Update", PackageOperation.Updated)]
    [InlineData("Remove", PackageOperation.Removed)]
    public async Task Operation_Success_PublishesPackageChangedEvent(
        string operation, PackageOperation expected)
    {
        var (service, _, _, eventBus, queue, notifications) = Create();

        switch (operation)
        {
            case "Install":
                await service.InstallAsync("Git.Git");
                break;
            case "Update":
                await service.UpdateAsync("Git.Git");
                break;
            case "Remove":
                await service.RemoveAsync("Git.Git");
                break;
        }

        var published = Assert.Single(eventBus.Published);
        var packageChanged = Assert.IsType<PackageChangedEvent>(published);
        Assert.Equal("Git.Git", packageChanged.PackageId);
        Assert.Equal(expected, packageChanged.Operation);
    }

    [Fact]
    public async Task Operation_Failed_DoesNotPublishEvent()
    {
        var (service, provider, _, eventBus, queue, notifications) = Create();
        provider.Throws = true;

        await service.InstallAsync("Git.Git");

        Assert.Empty(eventBus.Published);
    }

    [Fact]
    public async Task BlankPackageId_DoesNotPublishEvent()
    {
        var (service, _, _, eventBus, queue, notifications) = Create();

        await service.InstallAsync("   ");

        Assert.Empty(eventBus.Published);
    }

    [Fact]
    public async Task Operations_AreRoutedThroughQueue()
    {
        var (service, _, _, _, queue, _) = Create();

        await service.InstallAsync("Git.Git");

        var item = Assert.Single(queue.Enqueued);
        Assert.Equal("Git.Git", item.PackageId);
        Assert.Equal(PackageOperation.Installed, item.Operation);
        Assert.Equal(OperationStatus.Completed, item.Status);
    }

    [Fact]
    public async Task Operation_Success_CreatesSuccessNotification()
    {
        var (service, _, _, _, _, notifications) = Create();

        await service.InstallAsync("Git.Git");

        var notification = Assert.Single(notifications.Notifications);
        Assert.Equal(NotificationType.Success, notification.Type);
        Assert.Equal("Git.Git Installed", notification.Title);
        Assert.Equal("Git.Git installed successfully.", notification.Message);
    }

    [Fact]
    public async Task Operation_Failed_CreatesErrorNotification()
    {
        var (service, provider, _, _, _, notifications) = Create();
        provider.Throws = true;

        await service.InstallAsync("Git.Git");

        var notification = Assert.Single(notifications.Notifications);
        Assert.Equal(NotificationType.Error, notification.Type);
        Assert.Equal("Git.Git Failed", notification.Title);
        Assert.Equal("Unable to install Git.Git.\n\nReason:\n\nboom", notification.Message);
    }
}
