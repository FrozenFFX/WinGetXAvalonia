using WinGetXAvalonia.Core.PackageEngine;
using WinGetXAvalonia.Services.Operations;

namespace WinGetXAvalonia.Tests;

public class PackageEngineTests
{
    private sealed class FakeOperationService : IOperationService
    {
        public List<string> Installs { get; } = new();

        public List<string> Updates { get; } = new();

        public List<string> Removes { get; } = new();

        public int UpgradeAllCalls { get; private set; }

        public bool ThrowOnCall { get; set; }

        public Task InstallAsync(string packageId)
        {
            Installs.Add(packageId);
            return Done();
        }

        public Task UpdateAsync(string packageId)
        {
            Updates.Add(packageId);
            return Done();
        }

        public Task RemoveAsync(string packageId)
        {
            Removes.Add(packageId);
            return Done();
        }

        public Task UpgradeAllAsync()
        {
            UpgradeAllCalls++;
            return Done();
        }

        private Task Done()
            => ThrowOnCall ? Task.FromException(new InvalidOperationException("boom")) : Task.CompletedTask;
    }

    private static (PackageEngine engine, FakeOperationService operations) Create()
    {
        var operations = new FakeOperationService();

        return (new PackageEngine(operations), operations);
    }

    [Fact]
    public async Task ExecuteAsync_Install_CallsInstall()
    {
        var (engine, operations) = Create();

        var result = await engine.ExecuteAsync(new PackageRequest
        {
            PackageId = "Git.Git",
            Operation = PackageOperation.Install
        });

        Assert.True(result.Success);
        Assert.Contains("Git.Git", operations.Installs);
        Assert.True(result.Duration >= TimeSpan.Zero);
        Assert.True(result.Finished >= result.Started);
    }

    [Fact]
    public async Task ExecuteAsync_Update_CallsUpdate()
    {
        var (engine, operations) = Create();

        await engine.ExecuteAsync(new PackageRequest
        {
            PackageId = "Git.Git",
            Operation = PackageOperation.Update
        });

        Assert.Contains("Git.Git", operations.Updates);
    }

    [Fact]
    public async Task ExecuteAsync_Remove_CallsRemove()
    {
        var (engine, operations) = Create();

        await engine.ExecuteAsync(new PackageRequest
        {
            PackageId = "Git.Git",
            Operation = PackageOperation.Remove
        });

        Assert.Contains("Git.Git", operations.Removes);
    }

    [Fact]
    public async Task ExecuteAsync_UpgradeAll_CallsUpgradeAll()
    {
        var (engine, operations) = Create();

        await engine.ExecuteAsync(new PackageRequest
        {
            Operation = PackageOperation.UpgradeAll
        });

        Assert.Equal(1, operations.UpgradeAllCalls);
    }

    [Fact]
    public async Task ExecuteAsync_Repair_Unsupported_ReturnsFailure()
    {
        var (engine, operations) = Create();

        var result = await engine.ExecuteAsync(new PackageRequest
        {
            PackageId = "Git.Git",
            Operation = PackageOperation.Repair
        });

        Assert.False(result.Success);
        Assert.Empty(operations.Installs);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOperationThrows_ReturnsFailureWithOutput()
    {
        var (engine, operations) = Create();
        operations.ThrowOnCall = true;

        var result = await engine.ExecuteAsync(new PackageRequest
        {
            PackageId = "Git.Git",
            Operation = PackageOperation.Remove
        });

        Assert.False(result.Success);
        Assert.Equal("boom", result.Output);
    }
}
