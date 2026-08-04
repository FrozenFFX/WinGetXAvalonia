using WinGetXAvalonia.Core.PackageEngine;
using WinGetXAvalonia.Models.Operations;
using WinGetXAvalonia.Services.Operations;
using WinGetXAvalonia.Services.Queue;

namespace WinGetXAvalonia.Tests;

public class OperationCenterTests
{
    private readonly OperationCenter _center = new();

    [Fact]
    public void Start_AddsRunningOperation()
    {
        var op = _center.Start("Google Chrome", "Downloading...");

        Assert.Single(_center.Operations);
        Assert.Equal("Google Chrome", op.Title);
        Assert.Equal("Downloading...", op.Subtitle);
        Assert.Equal(OperationStatus.Running, op.Status);
        Assert.Equal(0, op.Progress);
    }

    [Fact]
    public void Start_InsertsNewestFirst()
    {
        _center.Start("first", "");
        _center.Start("second", "");

        Assert.Equal("second", _center.Operations[0].Title);
        Assert.Equal(2, _center.Operations.Count);
    }

    [Fact]
    public void ReportProgress_UpdatesProgressAndSubtitle()
    {
        var op = _center.Start("Google Chrome", "Downloading...");

        _center.ReportProgress(op.Id, 50, "Half completed");

        Assert.Equal(50, op.Progress);
        Assert.Equal("Half completed", op.Subtitle);
    }

    [Fact]
    public void Complete_SetsProgress100_AndCompletedStatus()
    {
        var op = _center.Start("Google Chrome", "Downloading...");

        _center.ReportProgress(op.Id, 50, "Half completed");
        _center.Complete(op.Id);

        Assert.Equal(100, op.Progress);
        Assert.Equal(OperationStatus.Completed, op.Status);
    }

    [Fact]
    public void Fail_SetsFailedStatus_AndErrorSubtitle()
    {
        var op = _center.Start("Google Chrome", "Downloading...");

        _center.Fail(op.Id, "Download failed");

        Assert.Equal(OperationStatus.Failed, op.Status);
        Assert.Equal("Download failed", op.Subtitle);
    }

    [Fact]
    public void Remove_RemovesOperation()
    {
        var op = _center.Start("Google Chrome", "");

        _center.Remove(op.Id);

        Assert.Empty(_center.Operations);
    }

    [Fact]
    public void ClearCompleted_RemovesOnlyCompleted()
    {
        var done = _center.Start("done", "");
        var failed = _center.Start("failed", "");
        var running = _center.Start("running", "");
        _center.Complete(done.Id);
        _center.Fail(failed.Id, "boom");

        _center.ClearCompleted();

        Assert.DoesNotContain(done, _center.Operations);
        Assert.Contains(failed, _center.Operations);
        Assert.Contains(running, _center.Operations);
        Assert.Equal(2, _center.Operations.Count);
    }

    [Fact]
    public void Methods_WithUnknownId_DoNotThrow()
    {
        _center.ReportProgress(Guid.NewGuid(), 50, "");
        _center.Complete(Guid.NewGuid());
        _center.Fail(Guid.NewGuid(), "boom");
        _center.Remove(Guid.NewGuid());
    }

    [Fact]
    public async Task OperationQueue_PublishesCompletedOperation()
    {
        var queue = new OperationQueue(
            new FakeEngine(),
            _center);

        await queue.EnqueueAsync(new PackageRequest
        {
            PackageId = "Git.Git",
            Operation = PackageOperation.Update
        });

        var op = Assert.Single(_center.Operations);
        Assert.Equal("Git.Git", op.Title);
        Assert.Equal("Updating...", op.Subtitle);
        Assert.Equal(OperationStatus.Completed, op.Status);
        Assert.Equal(100, op.Progress);
    }

    private sealed class FakeEngine : IPackageEngine
    {
        public Task<PackageResult> ExecuteAsync(PackageRequest request)
            => Task.FromResult(new PackageResult { Success = true });
    }
}
