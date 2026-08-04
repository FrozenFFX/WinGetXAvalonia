// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Collections.ObjectModel;
using WinGetXAvalonia.Core.PackageEngine;
using WinGetXAvalonia.Models.Operations;
using WinGetXAvalonia.Services.Operations;

namespace WinGetXAvalonia.Services.Queue;

public class OperationQueue : IOperationQueue
{
    private readonly IPackageEngine _engine;
    private readonly IOperationCenter _operationCenter;

    public ObservableCollection<QueueItem> Items { get; }
        = new();

    private bool _running;

    public OperationQueue(IPackageEngine engine, IOperationCenter operationCenter)
    {
        _engine = engine;
        _operationCenter = operationCenter;
    }

    public async Task EnqueueAsync(PackageRequest request)
    {
        Items.Add(new QueueItem
        {
            Request = request,
            Status = QueueStatus.Waiting
        });

        if (!_running)
            await StartAsync();
    }

    public async Task StartAsync()
    {
        if (_running)
            return;

        _running = true;

        while (true)
        {
            var item = Items.FirstOrDefault(x => x.Status == QueueStatus.Waiting);

            if (item == null)
                break;

            item.Status = QueueStatus.Running;
            item.Started = DateTime.Now;

            var op = _operationCenter.Start(
                item.Request.PackageId,
                Subtitle(item.Request.Operation));

            try
            {
                var result = await _engine.ExecuteAsync(item.Request);

                if (result.Success)
                {
                    item.Status = QueueStatus.Completed;
                    _operationCenter.Complete(op.Id);
                }
                else
                {
                    item.Status = QueueStatus.Failed;
                    item.Error = string.IsNullOrWhiteSpace(result.Output)
                        ? "Operation failed"
                        : result.Output;
                    _operationCenter.Fail(op.Id, item.Error);
                }
            }
            catch (Exception ex)
            {
                item.Status = QueueStatus.Failed;
                item.Error = ex.Message;
                _operationCenter.Fail(op.Id, ex.Message);
            }

            item.Finished = DateTime.Now;
        }

        _running = false;
    }

    private static string Subtitle(PackageOperation operation)
        => operation switch
        {
            PackageOperation.Install => "Installing...",
            PackageOperation.Update => "Updating...",
            PackageOperation.Remove => "Removing...",
            PackageOperation.Repair => "Repairing...",
            PackageOperation.UpgradeAll => "Upgrading all...",
            _ => "Working..."
        };

    public void Cancel(Guid id)
    {
        var item = Items.FirstOrDefault(x => x.Id == id);

        if (item != null && item.Status == QueueStatus.Waiting)
            item.Status = QueueStatus.Cancelled;
    }

    public void ClearCompleted()
    {
        foreach (var item in Items
                     .Where(x => x.Status == QueueStatus.Completed)
                     .ToList())
        {
            Items.Remove(item);
        }
    }
}
