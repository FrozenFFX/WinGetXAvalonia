// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System;
using System.Threading.Tasks;
using WinGetXAvalonia.Core.Events;
using WinGetXAvalonia.Core.Operations;
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
        => RunAsync("Install", packageId, PackageOperation.Installed, progress => _provider.InstallAsync(packageId, progress));

    public Task UpdateAsync(string packageId)
        => RunAsync("Update", packageId, PackageOperation.Updated, progress => _provider.UpgradeAsync(packageId, progress));

    public Task RemoveAsync(string packageId)
        => RunAsync("Remove", packageId, PackageOperation.Removed, progress => _provider.UninstallAsync(packageId, progress));

    public async Task UpgradeAllAsync()
    {
        var updates = await _provider.GetUpdatesAsync();

        foreach (var update in updates
                     .Where(x => !string.IsNullOrWhiteSpace(x.Id)))
        {
            await RunAsync(
                "Update",
                update.Id,
                PackageOperation.Updated,
                progress => _provider.UpgradeAsync(update.Id, progress));
        }
    }

    private async Task RunAsync(
        string operation,
        string packageId,
        PackageOperation eventOperation,
        Func<IOperationProgress?, Task> work)
    {
        if (string.IsNullOrWhiteSpace(packageId))
            return;

        packageId = packageId.Trim();

        var active = _center.Start(operation, packageId);
        active.StartedAt = DateTime.UtcNow;
        active.Progress = 0;
        active.StatusMessage = "Preparing...";

        var reporter = new OperationProgressReporter(active);

        bool success;
        string? message = null;

        try
        {
            reporter.Report(0, "Starting");

            await work(reporter);

            reporter.Report(100, "Completed");

            active.Progress = 100;
            active.StatusMessage = "Completed";
            _center.Complete(active.Id);
            success = true;
        }
        catch (Exception ex)
        {
            reporter.Report((int)active.Progress, ex.Message);

            active.StatusMessage = ex.Message;
            _center.Fail(active.Id, ex.Message);
            success = false;
            message = ex.Message;
        }

        active.CompletedAt = DateTime.UtcNow;

        if (!success && message is not null)
            _logger.Error(new InvalidOperationException(message), $"Operation '{operation}' failed for '{packageId}'");

        await _history.AddAsync(new HistoryEntry
        {
            Timestamp = DateTime.UtcNow,
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
