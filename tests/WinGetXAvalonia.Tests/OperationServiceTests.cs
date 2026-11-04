// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using WinGetXAvalonia.Core.Events;
using WinGetXAvalonia.Core.Operations;
using WinGetXAvalonia.Core.Providers;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Models.Operations;
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

        public HashSet<string> FailingUpgrades { get; } = new(StringComparer.OrdinalIgnoreCase);

        public bool HoldUpgrades { get; set; }

        public TaskCompletionSource UpgradeStarted { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource UpgradeRelease { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Action<string>? OnUpgrade { get; set; }

        public Task<IReadOnlyList<PackageModel>> SearchAsync(string query)
            => Task.FromResult<IReadOnlyList<PackageModel>>(Array.Empty<PackageModel>());

        public Task<IReadOnlyList<InstalledPackage>> GetInstalledAsync()
            => Task.FromResult<IReadOnlyList<InstalledPackage>>(Array.Empty<InstalledPackage>());

        public Task<IReadOnlyList<UpdatePackage>> GetUpdatesAsync()
            => Task.FromResult<IReadOnlyList<UpdatePackage>>(Updates.ToArray());

        public async Task InstallAsync(string id, IOperationProgress? progress = null)
        {
            Installed.Add(id);

            if (Throws)
                throw new InvalidOperationException("boom");
        }

        public async Task UpgradeAsync(string id, IOperationProgress? progress = null)
        {
            Upgraded.Add(id);

            OnUpgrade?.Invoke(id);

            UpgradeStarted.TrySetResult();

            if (HoldUpgrades)
                await UpgradeRelease.Task;

            if (FailingUpgrades.Contains(id))
                throw new InvalidOperationException($"upgrade failed: {id}");
        }

        public Task UninstallAsync(string id, IOperationProgress? progress = null)
        {
            Uninstalled.Add(id);
            return Task.CompletedTask;
        }

        public Task<string?> GetHomepageAsync(string id)
            => Task.FromResult<string?>(null);

        public Task<PackageDetails> GetDetailsAsync(string id)
            => Task.FromResult(new PackageDetails());
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

    private static (OperationService Service, FakeProvider Provider, HistoryService History, FakeEventBus Events, NotificationService Notifications, OperationCenter Center) Create()
    {
        var provider = new FakeProvider();
        var history = new HistoryService(CreateTempFile());
        var events = new FakeEventBus();
        var notifications = new NotificationService();
        var center = new OperationCenter();
        var service = new OperationService(provider, history, new LoggerService(), events, notifications, center);
        return (service, provider, history, events, notifications, center);
    }

    private static async Task<List<HistoryEntry>> GetEntriesAsync(HistoryService history)
        => (await history.GetAllAsync()).ToList();

    [Fact]
    public async Task InstallAsync_CallsProvider_AndRecordsHistory()
    {
        var (service, provider, history, eventBus, notifications, _) = Create();

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
        var (service, provider, history, eventBus, notifications, _) = Create();

        await service.UpdateAsync("Git.Git");

        Assert.Contains("Git.Git", provider.Upgraded);

        var entry = Assert.Single(await GetEntriesAsync(history));
        Assert.Equal("Update", entry.Operation);
        Assert.True(entry.Success);
    }

    [Fact]
    public async Task RemoveAsync_CallsUninstall_AndRecordsHistory()
    {
        var (service, provider, history, eventBus, notifications, _) = Create();

        await service.RemoveAsync("Git.Git");

        Assert.Contains("Git.Git", provider.Uninstalled);

        var entry = Assert.Single(await GetEntriesAsync(history));
        Assert.Equal("Remove", entry.Operation);
        Assert.True(entry.Success);
    }

    [Fact]
    public async Task Operation_Failed_RecordsFailure_WithoutThrowing()
    {
        var (service, provider, history, eventBus, notifications, _) = Create();
        provider.Throws = true;

        await service.InstallAsync("Git.Git");

        var entry = Assert.Single(await GetEntriesAsync(history));
        Assert.False(entry.Success);
        Assert.Equal("boom", entry.Message);
    }

    [Fact]
    public async Task BlankPackageId_SkipsProviderAndHistory()
    {
        var (service, provider, history, eventBus, notifications, _) = Create();

        await service.InstallAsync("   ");

        Assert.Empty(provider.Installed);
        Assert.Empty(await GetEntriesAsync(history));
    }

    [Fact]
    public async Task UpgradeAllAsync_UpgradesEveryUpdate_AndRecordsPerPackage()
    {
        var (service, provider, history, eventBus, notifications, _) = Create();
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
        var (service, _, _, eventBus, notifications, _) = Create();

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
        var (service, provider, _, eventBus, notifications, _) = Create();
        provider.Throws = true;

        await service.InstallAsync("Git.Git");

        Assert.Empty(eventBus.Published);
    }

    [Fact]
    public async Task BlankPackageId_DoesNotPublishEvent()
    {
        var (service, _, _, eventBus, notifications, _) = Create();

        await service.InstallAsync("   ");

        Assert.Empty(eventBus.Published);
    }

    [Fact]
    public async Task Operation_Success_CreatesSuccessNotification()
    {
        var (service, _, _, _, notifications, _) = Create();

        await service.InstallAsync("Git.Git");

        var notification = Assert.Single(notifications.Notifications);
        Assert.Equal(NotificationType.Success, notification.Type);
        Assert.Equal("Git.Git Installed", notification.Title);
        Assert.Equal("Git.Git installed successfully.", notification.Message);
    }

    [Fact]
    public async Task Operation_Failed_CreatesErrorNotification()
    {
        var (service, provider, _, _, notifications, _) = Create();
        provider.Throws = true;

        await service.InstallAsync("Git.Git");

        var notification = Assert.Single(notifications.Notifications);
        Assert.Equal(NotificationType.Error, notification.Type);
        Assert.Equal("Git.Git Failed", notification.Title);
        Assert.Equal("Unable to install Git.Git.\n\nReason:\n\nboom", notification.Message);
    }

    [Fact]
    public async Task Operation_TracksProgressInCenter()
    {
        var (service, _, _, _, _, center) = Create();

        await service.InstallAsync("Git.Git");

        var operation = Assert.Single(center.Operations);
        Assert.Equal(OperationStatus.Completed, operation.Status);
        Assert.Equal(100, operation.Progress);
        Assert.Equal("Completed", operation.StatusMessage);
    }

    [Fact]
    public async Task Operation_Failed_ReportsErrorInCenter()
    {
        var (service, provider, _, _, _, center) = Create();
        provider.Throws = true;

        await service.InstallAsync("Git.Git");

        var operation = Assert.Single(center.Operations);
        Assert.Equal(OperationStatus.Failed, operation.Status);
        Assert.Equal("boom", operation.StatusMessage);
    }

    [Fact]
    public async Task UpdateAllAsync_UpdatesExactlyRequestedIds()
    {
        var (service, provider, history, _, _, _) = Create();
        provider.Updates =
        [
            new UpdatePackage { Id = "Git.Git" },
            new UpdatePackage { Id = "Microsoft.VSCode" },
            new UpdatePackage { Id = "Mozilla.Firefox" }
        ];

        var result = await service.UpdateAllAsync(new[] { "Git.Git", "Mozilla.Firefox" });

        Assert.Equal(2, result.Succeeded);
        Assert.False(result.IsCancelled);
        Assert.True(result.SucceededAll);

        Assert.Equal(2, provider.Upgraded.Count);
        Assert.Contains("Git.Git", provider.Upgraded);
        Assert.Contains("Mozilla.Firefox", provider.Upgraded);
        Assert.DoesNotContain("Microsoft.VSCode", provider.Upgraded);

        var entries = await GetEntriesAsync(history);
        Assert.Equal(2, entries.Count);
        Assert.All(entries, e => Assert.True(e.Success));
    }

    [Fact]
    public async Task UpdateAllAsync_DuplicateAndBlankIds_AreIgnored()
    {
        var (service, provider, _, _, _, _) = Create();

        var result = await service.UpdateAllAsync(
            new[] { "Git.Git", "Git.Git", " ", "  " });

        Assert.Equal(1, result.Succeeded);
        Assert.Equal("Git.Git", Assert.Single(provider.Upgraded));
    }

    [Fact]
    public async Task UpdateAllAsync_PartialFailure_IsNotReportedAsSuccess()
    {
        var (service, provider, history, _, notifications, _) = Create();
        provider.FailingUpgrades.Add("Microsoft.VSCode");

        var result = await service.UpdateAllAsync(
            new[] { "Git.Git", "Microsoft.VSCode", "Mozilla.Firefox" });

        Assert.Equal(2, result.Succeeded);
        Assert.Equal(1, result.Failed);
        Assert.False(result.SucceededAll);

        var entries = await GetEntriesAsync(history);
        Assert.Equal(3, entries.Count);
        Assert.Equal(2, entries.Count(e => e.Success));
        Assert.Equal(1, entries.Count(e => !e.Success));

        var summary = Assert.Single(notifications.Notifications);
        Assert.Equal(NotificationType.Warning, summary.Type);
    }

    [Fact]
    public async Task UpdateAllAsync_Cancelled_StopsBeforeRemainingPackages()
    {
        var (service, provider, _, _, _, _) = Create();

        using var cts = new CancellationTokenSource();
        provider.OnUpgrade = _ => cts.Cancel();

        var result = await service.UpdateAllAsync(
            new[] { "Git.Git", "Microsoft.VSCode", "Mozilla.Firefox" },
            ct: cts.Token);

        Assert.True(result.IsCancelled);
        Assert.Equal(1, result.Succeeded);
        Assert.Equal(1, result.CancelledCount);
        Assert.Equal("Git.Git", Assert.Single(provider.Upgraded));
    }

    [Fact]
    public async Task UpdateAllAsync_ConcurrentRun_Throws()
    {
        var (service, provider, _, _, _, _) = Create();
        provider.HoldUpgrades = true;

        var first = service.UpdateAllAsync(new[] { "Git.Git" });

        await provider.UpgradeStarted.Task;

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.UpdateAllAsync(new[] { "Microsoft.VSCode" }));

        provider.UpgradeRelease.TrySetResult();

        var result = await first;

        Assert.Equal(1, result.Succeeded);
    }

    [Fact]
    public async Task UpdateAllAsync_EmptyIds_ReturnsNone()
    {
        var (service, provider, history, _, _, _) = Create();

        var result = await service.UpdateAllAsync(Array.Empty<string>());

        Assert.Equal(0, result.Total);
        Assert.Empty(provider.Upgraded);
        Assert.Empty(await GetEntriesAsync(history));
    }
}
