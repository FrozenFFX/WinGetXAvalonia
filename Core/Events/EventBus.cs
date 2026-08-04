// ==========================================================
// WinGetX - https://github.com/FrozenFFX
// ==========================================================

using System.Collections.Concurrent;

namespace WinGetXAvalonia.Core.Events;

public sealed class EventBus : IEventBus
{
    private readonly ConcurrentDictionary<Type, List<Delegate>> _subscriptions = new();

    public void Publish<T>(T appEvent)
        where T : AppEvent
    {
        if (!_subscriptions.TryGetValue(typeof(T), out var handlers))
            return;

        foreach (var handler in handlers.ToArray())
        {
            ((Action<T>)handler).Invoke(appEvent);
        }
    }

    public IDisposable Subscribe<T>(Action<T> handler)
        where T : AppEvent
    {
        var list = _subscriptions.GetOrAdd(typeof(T), _ => []);

        lock (list)
        {
            list.Add(handler);
        }

        return new Subscription(() =>
        {
            lock (list)
            {
                list.Remove(handler);
            }
        });
    }

    private sealed class Subscription : IDisposable
    {
        private readonly Action _dispose;

        public Subscription(Action dispose)
        {
            _dispose = dispose;
        }

        public void Dispose()
        {
            _dispose();
        }
    }
}
