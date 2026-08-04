using System;
using System.Threading.Tasks;
using WinGetXAvalonia.Core.Events;
using WinGetXAvalonia.Core.Providers;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Core;
using WinGetXAvalonia.Services.History;
using WinGetXAvalonia.Services.Notifications;

namespace WinGetXAvalonia.Services.Operations;

public sealed class OperationService : IOperationService
{
    private readonly IPackageProvider _provider;
    private readonly IHistoryService _history;
    private readonly LoggerService _logger;
    private readonly IEventBus _eventBus;
    private readonly IOperationQueue _queue;
    private readonly NotificationService _notifications;

    public OperationService(
        IPackageProvider provider,
        IHistoryService history,
        LoggerService logger,
        IEventBus eventBus,
        IOperationQueue queue,
        NotificationService notifications)
    {
        _provider = provider;
        _history = history;
        _logger = logger;
        _eventBus = eventBus;
        _queue = queue;
        _notifications = notifications;
    }

    public Task InstallAsync(string packageId)
        => RunAsync("Install", packageId, PackageOperation.Installed, () => _provider.InstallAsync(packageId));

    public Task UpdateAsync(string packageId)
        => RunAsync("Update", packageId, PackageOperation.Updated, () => _provider.UpgradeAsync(packageId));

    public Task RemoveAsync(string packageId)
        => RunAsync("Remove", packageId, PackageOperation.Removed, () => _provider.UninstallAsync(packageId));

    public async Task UpgradeAllAsync()
    {
        foreach (var update in await _provider.GetUpdatesAsync())
            await RunAsync("Update", update.Id, PackageOperation.Updated, () => _provider.UpgradeAsync(update.Id));
    }

    private async Task RunAsync(
        string operation,
        string packageId,
        PackageOperation eventOperation,
        Func<Task> work)
    {
        if (string.IsNullOrWhiteSpace(packageId))
            return;

        var item = new OperationItem
        {
            PackageId = packageId,
            DisplayName = packageId,
            Operation = eventOperation
        };

        await _queue.EnqueueAsync(item, work);

        bool success = item.Status == OperationStatus.Completed;
        string? message = item.Status == OperationStatus.Failed ? item.Message : null;

        if (!success && message is not null)
            _logger.Error(new InvalidOperationException(message), $"Operation '{operation}' failed for '{packageId}'");

        await _history.AddAsync(new HistoryEntry
        {
            Timestamp = DateTime.Now,
            Operation = operation,
            PackageId = packageId,
            Success = success,
            Message = message
        });

        if (success)
        {
            _notifications.Notify(
                NotificationType.Success,
                $"{packageId} {PastTense(eventOperation)}",
                $"{packageId} {PastTenseLower(eventOperation)} successfully.");

            _eventBus.Publish(new PackageChangedEvent
            {
                PackageId = packageId,
                Operation = eventOperation
            });
        }
        else
        {
            _notifications.Notify(
                NotificationType.Error,
                $"{packageId} Failed",
                $"Unable to {operation.ToLowerInvariant()} {packageId}.\n\nReason:\n\n{message ?? "Unknown error"}");
        }
    }

    private static string PastTense(PackageOperation operation)
        => operation switch
        {
            PackageOperation.Installed => "Installed",
            PackageOperation.Updated => "Updated",
            _ => "Removed"
        };

    private static string PastTenseLower(PackageOperation operation)
        => operation switch
        {
            PackageOperation.Installed => "installed",
            PackageOperation.Updated => "updated",
            _ => "removed"
        };
}
