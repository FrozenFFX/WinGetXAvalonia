using System.Collections.Specialized;
using WinGetXAvalonia.Collections;

namespace WinGetXAvalonia.Tests;

public class ObservableRangeCollectionTests
{
    [Fact]
    public void AddRange_RaisesSingleAddNotification()
    {
        var collection = new ObservableRangeCollection<int>();
        var notifications = new List<NotifyCollectionChangedEventArgs>();

        collection.CollectionChanged += (_, e) => notifications.Add(e);

        collection.AddRange(new[] { 1, 2, 3 });

        Assert.Equal(3, collection.Count);
        Assert.Single(notifications);
        Assert.Equal(NotifyCollectionChangedAction.Add, notifications[0].Action);
        Assert.Equal(3, notifications[0].NewItems!.Count);
    }

    [Fact]
    public void ReplaceAll_RaisesSingleResetNotification()
    {
        var collection = new ObservableRangeCollection<int> { 1, 2, 3 };
        var notifications = new List<NotifyCollectionChangedEventArgs>();

        collection.CollectionChanged += (_, e) => notifications.Add(e);

        collection.ReplaceAll(new[] { 4, 5 });

        Assert.Equal(new[] { 4, 5 }, collection);
        Assert.Single(notifications);
        Assert.Equal(NotifyCollectionChangedAction.Reset, notifications[0].Action);
    }

    [Fact]
    public void ReplaceAll_Empty_RaisesReset_AndEmpties()
    {
        var collection = new ObservableRangeCollection<int> { 1, 2 };

        collection.ReplaceAll(Array.Empty<int>());

        Assert.Empty(collection);
    }
}
