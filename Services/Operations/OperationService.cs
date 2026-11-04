// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
    private readonly SemaphoreSlim _bulkGate = new(1, 1);

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
        => RunAsync("Install", packageId, PackageOperation.Installed, progress => _provider.InstallAsync(packageId, progress), notify: true);

    public Task UpdateAsync(string packageId)
        => RunAsync("Update", packageId, PackageOperation.Updated, progress => _provider.UpgradeAsync(packageId, progress), notify: true);

    public Task RemoveAsync(string packageId)
        => RunAsync("Remove", packageId, PackageOperation.Removed, progress => _provider.UninstallAsync(packageId, progress), notify: true);

    public async Task<BulkUpdateResult> UpdateAllAsync(
        IReadOnlyList<string> packageIds,
        IProgress<BulkUpdateProgress>? progress = null,
        CancellationToken ct = default)
    {
        var ids = packageIds?
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList() ?? new List<string>();

        if (ids.Count == 0)
            return BulkUpdateResult.None;

        // Eşzamanlı Update All / Update Selected koşusu olmasın.
        // İlk çağrı kazanır; ikinci çağrı net hata döner.
        if (!await _bulkGate.WaitAsync(0, ct).ConfigureAwait(false))
            throw new InvalidOperationException("Another bulk update operation is already running.");

        var succeeded = 0;
        var failed = 0;
        var cancelled = 0;
        var wasCancelled = false;
        var completed = 0;

        try
        {
            foreach (var id in ids)
            {
                if (ct.IsCancellationRequested)
                {
                    cancelled++;
                    wasCancelled = true;
                    _logger.Log("Bulk update cancelled by user");
                    break;
                }

                var current = id;

                try
                {
                    var ok = await RunAsync(
                        "Update",
                        current,
                        PackageOperation.Updated,
                        p => _provider.UpgradeAsync(current, p),
                        notify: false).ConfigureAwait(false);

                    if (ok)
                        succeeded++;
                    else
                        failed++;
                }
                catch (OperationCanceledException)
                {
                    cancelled++;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Bulk update failed for '{current}'");
                    failed++;
                }

                completed++;

                progress?.Report(new BulkUpdateProgress
                {
                    Total = ids.Count,
                    Completed = completed,
                    Succeeded = succeeded,
                    Failed = failed,
                    CancelledCount = cancelled,
                    CurrentPackage = current
                });
            }
        }
        finally
        {
            _bulkGate.Release();
        }

        await RefreshPackageStates().ConfigureAwait(false);
        NotifyBulkResult(succeeded, failed, cancelled, wasCancelled);

        return new BulkUpdateResult
        {
            Succeeded = succeeded,
            Failed = failed,
            CancelledCount = cancelled,
            IsCancelled = wasCancelled
        };
    }

    public async Task<BulkUpdateResult> UpgradeAllAsync(
        IProgress<BulkUpdateProgress>? progress = null,
        CancellationToken ct = default)
    {
        var updates = await _provider.GetUpdatesAsync().ConfigureAwait(false);

        return await UpdateAllAsync(
                updates.Select(u => u.Id).ToList(),
                progress,
                ct)
            .ConfigureAwait(false);
    }

    private void NotifyBulkResult(int succeeded, int failed, int cancelled, bool wasCancelled)
    {
        if (wasCancelled && succeeded == 0 && failed == 0)
        {
            _notifications.Notify(
                NotificationType.Information,
                "Updates cancelled",
                "The update operation was cancelled.");
            return;
        }

        if (failed == 0 && cancelled == 0)
        {
            _notifications.Notify(
                NotificationType.Success,
                "Updates completed",
                $"{succeeded} packages updated successfully.");
            return;
        }

        _notifications.Notify(
            NotificationType.Warning,
            "Updates completed with some failures",
            $"{succeeded} successful, {failed} failed, {cancelled} cancelled.");
    }

    /// <summary>
    /// Tek paket işlemi. Başarı/başarısızlığı döndürür; history her koşulda
    /// yazılır. Uygunsa bildirim + event yayınlar.
    /// </summary>
    private async Task<bool> RunAsync(
        string operation,
        string packageId,
        PackageOperation eventOperation,
        Func<IOperationProgress?, Task> work,
        bool notify)
    {
        if (string.IsNullOrWhiteSpace(packageId))
            return false;

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

            await work(reporter).ConfigureAwait(false);

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
        }).ConfigureAwait(false);

        if (!notify)
            return success;

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

        return success;
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

    private async Task RefreshPackageStates()
    {
        // Installed + update cache'lerini tazele; ayrı ayrı winget koşturma.
        await _provider.GetInstalledAsync().ConfigureAwait(false);
        await _provider.GetUpdatesAsync().ConfigureAwait(false);
    }
}