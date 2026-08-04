// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

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
    private readonly NotificationService _notifications;
    private readonly IOperationCenter _center;

    public OperationService(
        IPackageProvider provider,
        IHistoryService history,
        LoggerService logger,
        IEventBus eventBus,
        NotificationService notifications,
        IOperationCenter center)
    {
        _provider = provider;
        _history = history;
        _logger = logger;
        _eventBus = eventBus;
        _notifications = notifications;
        _center = center;
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

        var active = _center.Start(operation, packageId);
        active.StatusMessage = "Starting...";

        bool success;
        string? message = null;

        try
        {
            await work();
            active.Progress = 100;
            active.StatusMessage = "Completed";
            _center.Complete(active.Id);
            success = true;
        }
        catch (Exception ex)
        {
            active.StatusMessage = ex.Message;
            _center.Fail(active.Id, ex.Message);
            success = false;
            message = ex.Message;
        }

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
