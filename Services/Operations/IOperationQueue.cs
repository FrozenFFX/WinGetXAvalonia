// ==========================================================
// WinGetX - https://github.com/FrozenFFX
// ==========================================================

namespace WinGetXAvalonia.Services.Operations;

// kaneki
public interface IOperationQueue
{
    IReadOnlyList<OperationItem> Operations { get; }

    Task EnqueueAsync(OperationItem item, Func<Task> action);

    event EventHandler? QueueChanged;
}
// kaneki
