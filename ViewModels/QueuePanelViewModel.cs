using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using WinGetXAvalonia.Services.Operations;

namespace WinGetXAvalonia.ViewModels;

public partial class QueuePanelViewModel : ViewModelBase
{
    private readonly IOperationQueue _queue;

    public ObservableCollection<OperationItem> Items { get; } = new();

    [ObservableProperty]
    private int runningCount;

    [ObservableProperty]
    private int waitingCount;

    [ObservableProperty]
    private int completedCount;

    [ObservableProperty]
    private int failedCount;

    [ObservableProperty]
    private int cancelledCount;

    [ObservableProperty]
    private bool isEmpty = true;

    public QueuePanelViewModel(IOperationQueue queue)
    {
        _queue = queue;
        _queue.QueueChanged += OnQueueChanged;
        Refresh();
    }

    private void OnQueueChanged(object? sender, EventArgs e)
        => Refresh();

    public void Refresh()
    {
        var snapshot = _queue.Operations;

        foreach (var item in snapshot)
        {
            if (!Items.Contains(item))
                Items.Add(item);
        }

        for (int i = Items.Count - 1; i >= 0; i--)
        {
            if (!snapshot.Contains(Items[i]))
                Items.RemoveAt(i);
        }

        RunningCount = CountOf(snapshot, OperationStatus.Running);
        WaitingCount = CountOf(snapshot, OperationStatus.Pending);
        CompletedCount = CountOf(snapshot, OperationStatus.Completed);
        FailedCount = CountOf(snapshot, OperationStatus.Failed);
        CancelledCount = CountOf(snapshot, OperationStatus.Cancelled);

        IsEmpty = Items.Count == 0;
    }

    private static int CountOf(IReadOnlyList<OperationItem> items, OperationStatus status)
    {
        int count = 0;

        foreach (var item in items)
        {
            if (item.Status == status)
                count++;
        }

        return count;
    }
}
