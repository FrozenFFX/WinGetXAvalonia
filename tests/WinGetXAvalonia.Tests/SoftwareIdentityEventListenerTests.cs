// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using WinGetXAvalonia.Core.Events;
using WinGetXAvalonia.Services.Core;
using WinGetXAvalonia.Services.Identity;

namespace WinGetXAvalonia.Tests;

public class SoftwareIdentityEventListenerTests : IDisposable
{
    private readonly string _folder;

    public SoftwareIdentityEventListenerTests()
    {
        _folder = Path.Combine(
            Path.GetTempPath(),
            "WinGetXAvalonia.Tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_folder);
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(_folder, recursive: true);
        }
        catch (IOException)
        {
            // test temizliği ikincil; başarısızsa görmezden gel
        }
    }

    private (SoftwareIdentityEventListener Listener, SoftwareIdentityService Identity, EventBus Bus)
        Create()
    {
        var identity = new SoftwareIdentityService(_folder);
        var bus = new EventBus();
        var listener = new SoftwareIdentityEventListener(
            identity,
            bus,
            new LoggerService());
        return (listener, identity, bus);
    }

    private static PackageChangedEvent Install(string packageId)
        => new() { PackageId = packageId, Operation = PackageOperation.Installed };

    [Fact]
    public async Task InstalledEvent_CreatesIdentity()
    {
        var (listener, identity, _) = Create();

        await listener.HandleAsync(Install("Git.Git"));

        var all = await identity.GetAllAsync();
        var created = Assert.Single(all);
        Assert.Equal("Git.Git", created.Name);
        Assert.Equal("winget", created.Source);
    }

    [Fact]
    public async Task InstalledEvent_Twice_SingleProfile()
    {
        var (listener, identity, _) = Create();

        await listener.HandleAsync(Install("Git.Git"));
        await listener.HandleAsync(Install("Git.Git"));

        Assert.Single(await identity.GetAllAsync());
    }

    [Fact]
    public async Task UpdatedEvent_IncrementsUpdateCount()
    {
        var (listener, identity, _) = Create();

        await listener.HandleAsync(Install("Git.Git"));
        await listener.HandleAsync(
            new PackageChangedEvent { PackageId = "Git.Git", Operation = PackageOperation.Updated });

        var profile = Assert.Single(await identity.GetAllAsync());
        Assert.Equal(1, profile.UpdateCount);
    }

    [Fact]
    public async Task UpdatedEvent_UnknownPackage_DoesNothing()
    {
        var (listener, identity, _) = Create();

        await listener.HandleAsync(
            new PackageChangedEvent { PackageId = "Missing.App", Operation = PackageOperation.Updated });

        Assert.Empty(await identity.GetAllAsync());
    }

    [Fact]
    public async Task RemovedEvent_DeletesIdentity()
    {
        var (listener, identity, _) = Create();

        await listener.HandleAsync(Install("Git.Git"));
        await listener.HandleAsync(
            new PackageChangedEvent { PackageId = "Git.Git", Operation = PackageOperation.Removed });

        Assert.Empty(await identity.GetAllAsync());
    }

    [Fact]
    public async Task RemovedEvent_UnknownPackage_DoesNothing()
    {
        var (listener, identity, _) = Create();

        await listener.HandleAsync(
            new PackageChangedEvent { PackageId = "Missing.App", Operation = PackageOperation.Removed });

        Assert.Empty(await identity.GetAllAsync());
    }

    [Fact]
    public async Task PublishThroughBus_CreatesIdentity()
    {
        var (listener, identity, bus) = Create();

        bus.Publish(Install("Google.Chrome"));

        var profile = Assert.Single(await WaitForAsync(
            () => identity.GetAllAsync(),
            list => list.Count > 0));
        Assert.Equal("Google.Chrome", profile.Name);
    }

    private static async Task<IReadOnlyList<WinGetXAvalonia.Models.SoftwareIdentity>> WaitForAsync(
        Func<Task<IReadOnlyList<WinGetXAvalonia.Models.SoftwareIdentity>>> get,
        Func<IReadOnlyList<WinGetXAvalonia.Models.SoftwareIdentity>, bool> condition)
    {
        var deadline = DateTime.UtcNow.AddSeconds(5);

        while (DateTime.UtcNow < deadline)
        {
            var current = await get();

            if (condition(current))
                return current;

            await Task.Delay(20);
        }

        return await get();
    }
}
