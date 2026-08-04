// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using WinGetXAvalonia.Core.Events;

namespace WinGetXAvalonia.Tests;

public sealed class FakeEventBus : IEventBus
{
    public List<AppEvent> Published { get; } = new();

    public int SubscriberCount { get; private set; }

    public void Publish<T>(T appEvent)
        where T : AppEvent
    {
        Published.Add(appEvent);
    }

    public IDisposable Subscribe<T>(Action<T> handler)
        where T : AppEvent
    {
        SubscriberCount++;
        return new DisposableAction(() => SubscriberCount--);
    }

    private sealed class DisposableAction : IDisposable
    {
        private readonly Action _action;

        public DisposableAction(Action action)
        {
            _action = action;
        }

        public void Dispose() => _action();
    }
}
