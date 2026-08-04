// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using WinGetXAvalonia.Core.PackageEngine;
using WinGetXAvalonia.Services.Operations;
using WinGetXAvalonia.Services.Queue;

namespace WinGetXAvalonia.Tests;

public class OperationQueueTests
{
    private sealed class FakeEngine : IPackageEngine
    {
        private readonly Func<PackageRequest, Task<PackageResult>> _handler;

        public FakeEngine(Func<PackageRequest, Task<PackageResult>>? handler = null)
        {
            _handler = handler ?? (_ => Task.FromResult(new PackageResult { Success = true }));
        }

        public Task<PackageResult> ExecuteAsync(PackageRequest request)
            => _handler(request);
    }

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

    private static PackageRequest Request(string id = "Microsoft.VSCode")
        => new()
        {
            PackageId = id,
            Operation = PackageOperation.Update
        };

    [Fact]
    public async Task EnqueueAsync_AddsWaitingItem_AndRunsIt()
    {
        var queue = new OperationQueue(new FakeEngine(), new OperationCenter());

        await queue.EnqueueAsync(Request("Git.Git"));

        var item = Assert.Single(queue.Items);
        Assert.Equal(QueueStatus.Completed, item.Status);
        Assert.Equal("Git.Git", item.Request.PackageId);
        Assert.NotNull(item.Started);
        Assert.NotNull(item.Finished);
    }

    [Fact]
    public async Task EnqueueAsync_EngineFailure_MarksFailed_WithError()
    {
        var queue = new OperationQueue(new FakeEngine(_ =>
            Task.FromResult(new PackageResult { Success = false, Output = "nope" })), new OperationCenter());

        await queue.EnqueueAsync(Request());

        var item = Assert.Single(queue.Items);
        Assert.Equal(QueueStatus.Failed, item.Status);
        Assert.Equal("nope", item.Error);
    }

    [Fact]
    public async Task EnqueueAsync_EngineThrows_MarksFailed()
    {
        var queue = new OperationQueue(new FakeEngine(_ => throw new InvalidOperationException("boom")), new OperationCenter());

        await queue.EnqueueAsync(Request());

        var item = Assert.Single(queue.Items);
        Assert.Equal(QueueStatus.Failed, item.Status);
        Assert.Equal("boom", item.Error);
    }

    [Fact]
    public async Task EnqueueAsync_SerializesOperations_OneAtATime()
    {
        var blocking = new TaskCompletionSource();
        var started = 0;

        var engine = new FakeEngine(async _ =>
        {
            Interlocked.Increment(ref started);

            await blocking.Task;

            return new PackageResult { Success = true };
        });

        var queue = new OperationQueue(engine, new OperationCenter());

        var firstTask = queue.EnqueueAsync(Request("first"));
        await WaitUntilAsync(() => queue.Items.Any(x => x.Status == QueueStatus.Running));

        var secondTask = queue.EnqueueAsync(Request("second"));

        Assert.Equal(1, started);

        blocking.SetResult();
        await Task.WhenAll(firstTask, secondTask);

        Assert.Equal(2, started);
        Assert.All(queue.Items, item => Assert.Equal(QueueStatus.Completed, item.Status));
    }

    [Fact]
    public async Task Cancel_WaitingItem_OnlyCancelsThatItem()
    {
        var gate = new TaskCompletionSource();
        var queue = new OperationQueue(new FakeEngine(async _ =>
        {
            await gate.Task;
            return new PackageResult { Success = true };
        }), new OperationCenter());

        var firstTask = queue.EnqueueAsync(Request("first"));
        await WaitUntilAsync(() => queue.Items.Any(x => x.Status == QueueStatus.Running));

        var secondTask = queue.EnqueueAsync(Request("second"));
        var second = queue.Items.First(x => x.Request.PackageId == "second");

        queue.Cancel(second.Id);

        Assert.Equal(QueueStatus.Cancelled, second.Status);

        gate.SetResult();
        await Task.WhenAll(firstTask, secondTask);
    }

    [Fact]
    public async Task ClearCompleted_RemovesOnlyCompleted()
    {
        var gate = new TaskCompletionSource();
        var queue = new OperationQueue(new FakeEngine(async _ =>
        {
            await gate.Task;
            return new PackageResult { Success = true };
        }), new OperationCenter());

        var firstTask = queue.EnqueueAsync(Request("a"));
        await WaitUntilAsync(() => queue.Items.Any(x => x.Status == QueueStatus.Running));

        var secondTask = queue.EnqueueAsync(Request("b"));
        var second = queue.Items.First(x => x.Request.PackageId == "b");
        queue.Cancel(second.Id);

        gate.SetResult();
        await Task.WhenAll(firstTask, secondTask);

        queue.ClearCompleted();

        var remaining = Assert.Single(queue.Items);
        Assert.Equal("b", remaining.Request.PackageId);
        Assert.Equal(QueueStatus.Cancelled, remaining.Status);
    }
}
