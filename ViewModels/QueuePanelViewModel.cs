using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using WinGetXAvalonia.Services.Queue;

namespace WinGetXAvalonia.ViewModels;

public partial class QueuePanelViewModel : ViewModelBase
{
    private readonly IOperationQueue _queue;

    public ObservableCollection<QueueItem> Items { get; }

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
        Items = _queue.Items;

        Items.CollectionChanged += OnItemsChanged;

        foreach (var item in Items)
            item.PropertyChanged += OnItemChanged;

        Refresh();
    }

    private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (QueueItem item in e.OldItems)
                item.PropertyChanged -= OnItemChanged;
        }

        if (e.NewItems is not null)
        {
            foreach (QueueItem item in e.NewItems)
                item.PropertyChanged += OnItemChanged;
        }

        Refresh();
    }

    private void OnItemChanged(object? sender, PropertyChangedEventArgs e)
        => Refresh();

    private void Refresh()
    {
        RunningCount = CountOf(Items, QueueStatus.Running);
        WaitingCount = CountOf(Items, QueueStatus.Waiting);
        CompletedCount = CountOf(Items, QueueStatus.Completed);
        FailedCount = CountOf(Items, QueueStatus.Failed);
        CancelledCount = CountOf(Items, QueueStatus.Cancelled);

        IsEmpty = Items.Count == 0;
    }

    private static int CountOf(IEnumerable<QueueItem> items, QueueStatus status)
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
