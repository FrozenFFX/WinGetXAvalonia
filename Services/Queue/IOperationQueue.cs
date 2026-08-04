using System.Collections.ObjectModel;
using WinGetXAvalonia.Core.PackageEngine;

namespace WinGetXAvalonia.Services.Queue;

public interface IOperationQueue
{
    ObservableCollection<QueueItem> Items { get; }

    Task EnqueueAsync(PackageRequest request);

    Task StartAsync();

    void Cancel(Guid id);

    void ClearCompleted();
}
