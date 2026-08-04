using System.Collections.ObjectModel;

namespace WinGetXAvalonia.Services.Operations;

// kaneki
public sealed class OperationQueue : IOperationQueue
{
    private readonly ObservableCollection<OperationItem> _operations = new();

    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public IReadOnlyList<OperationItem> Operations => _operations;

    public event EventHandler? QueueChanged;

    public async Task EnqueueAsync(
        OperationItem item,
        Func<Task> action)
    {
        _operations.Add(item);

        QueueChanged?.Invoke(this, EventArgs.Empty);

        await _semaphore.WaitAsync();

        try
        {
            item.Status = OperationStatus.Running;

            QueueChanged?.Invoke(this, EventArgs.Empty);

            await action();

            item.Progress = 100;
            item.Status = OperationStatus.Completed;
        }
        catch (Exception ex)
        {
            item.Status = OperationStatus.Failed;
            item.Message = ex.Message;
        }
        finally
        {
            QueueChanged?.Invoke(this, EventArgs.Empty);

            _semaphore.Release();
        }
    }
}
