using WinGetXAvalonia.Core.Events;
using WinGetXAvalonia.Services.Operations;

namespace WinGetXAvalonia.Tests;

public class OperationQueueTests
{
    private static async Task WaitUntilAsync(Func<bool> condition, int timeoutMs = 5000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);

        while (!condition())
        {
            if (DateTime.UtcNow > deadline)
                throw new TimeoutException("Timed out waiting for condition");

            await Task.Delay(20);
        }
    }

    private static OperationItem Item(string id = "Microsoft.VSCode")
        => new()
        {
            PackageId = id,
            DisplayName = id,
            Operation = PackageOperation.Installed
        };

    [Fact]
    public async Task EnqueueAsync_RunsAction_AndCompletes()
    {
        var queue = new OperationQueue();
        var ran = false;

        await queue.EnqueueAsync(Item(), () =>
        {
            ran = true;
            return Task.CompletedTask;
        });

        Assert.True(ran);

        var item = Assert.Single(queue.Operations);
        Assert.Equal(OperationStatus.Completed, item.Status);
        Assert.Equal(100, item.Progress);
    }

    [Fact]
    public async Task EnqueueAsync_SetsFailed_OnActionError()
    {
        var queue = new OperationQueue();
        var item = Item();

        await queue.EnqueueAsync(item, () => throw new InvalidOperationException("boom"));

        Assert.Equal(OperationStatus.Failed, item.Status);
        Assert.Equal("boom", item.Message);
    }

    [Fact]
    public async Task EnqueueAsync_SerializesOperations_OneAtATime()
    {
        var queue = new OperationQueue();
        var blocking = new TaskCompletionSource();
        var first = Item("first");
        var second = Item("second");
        var secondRan = false;

        var firstTask = queue.EnqueueAsync(first, async () => await blocking.Task);
        await WaitUntilAsync(() => first.Status == OperationStatus.Running);

        var secondTask = queue.EnqueueAsync(second, () =>
        {
            secondRan = true;
            return Task.CompletedTask;
        });

        Assert.Equal(OperationStatus.Pending, second.Status);

        blocking.SetResult();
        await Task.WhenAll(firstTask, secondTask);

        Assert.Equal(OperationStatus.Completed, first.Status);
        Assert.Equal(OperationStatus.Completed, second.Status);
        Assert.True(secondRan);
    }

    [Fact]
    public async Task QueueChanged_Fires_OnStatusTransitions()
    {
        var queue = new OperationQueue();
        var notifications = 0;

        queue.QueueChanged += (_, _) => notifications++;

        await queue.EnqueueAsync(Item(), () => Task.CompletedTask);

        // enqueue + running + completed
        Assert.True(notifications >= 3);
    }

    [Fact]
    public async Task Operations_ExposesEnqueuedItems_BeforeCompletion()
    {
        var queue = new OperationQueue();
        var blocking = new TaskCompletionSource();
        var first = Item("first");

        var task = queue.EnqueueAsync(first, async () => await blocking.Task);
        await WaitUntilAsync(() => first.Status == OperationStatus.Running);

        Assert.Single(queue.Operations);

        blocking.SetResult();
        await task;
    }
}
