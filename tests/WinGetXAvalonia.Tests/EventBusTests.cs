// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using WinGetXAvalonia.Core.Events;

namespace WinGetXAvalonia.Tests;

public class EventBusTests
{
    [Fact]
    public void Publish_CallsAllSubscribers()
    {
        var bus = new EventBus();
        var received = new List<PackageChangedEvent>();

        bus.Subscribe<PackageChangedEvent>(e => received.Add(e));
        bus.Subscribe<PackageChangedEvent>(e => received.Add(e));

        bus.Publish(new PackageChangedEvent { PackageId = "Git.Git", Operation = PackageOperation.Installed });

        Assert.Equal(2, received.Count);
        Assert.All(received, e =>
        {
            Assert.Equal("Git.Git", e.PackageId);
            Assert.Equal(PackageOperation.Installed, e.Operation);
        });
    }

    [Fact]
    public void Publish_OnlyCallsMatchingTypeSubscribers()
    {
        var bus = new EventBus();
        var packageEvents = new List<PackageChangedEvent>();
        var baseEvents = new List<AppEvent>();

        bus.Subscribe<PackageChangedEvent>(e => packageEvents.Add(e));
        bus.Subscribe<AppEvent>(e => baseEvents.Add(e));

        bus.Publish(new PackageChangedEvent { PackageId = "Git.Git" });

        Assert.Single(packageEvents);
        Assert.Empty(baseEvents);
    }

    [Fact]
    public void Dispose_RemovesSubscription()
    {
        var bus = new EventBus();
        var received = new List<PackageChangedEvent>();

        using (bus.Subscribe<PackageChangedEvent>(e => received.Add(e)))
        {
            bus.Publish(new PackageChangedEvent());
            Assert.Single(received);
        }

        bus.Publish(new PackageChangedEvent());

        Assert.Single(received);
    }

    [Fact]
    public void Publish_WithNoSubscribers_DoesNotThrow()
    {
        var bus = new EventBus();

        bus.Publish(new PackageChangedEvent());
    }

    [Fact]
    public void ThrowingHandler_PropagatesAndStopsLaterSubscribers()
    {
        var bus = new EventBus();
        var received = new List<PackageChangedEvent>();

        bus.Subscribe<PackageChangedEvent>(e => received.Add(e));
        bus.Subscribe<PackageChangedEvent>(e => throw new InvalidOperationException("boom"));

        Assert.Throws<InvalidOperationException>(() => bus.Publish(new PackageChangedEvent()));
        Assert.Single(received);
    }
}
