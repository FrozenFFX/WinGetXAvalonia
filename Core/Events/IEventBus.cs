// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

// ==========================================================
// WinGetX - https://github.com/FrozenFFX
// ==========================================================

namespace WinGetXAvalonia.Core.Events;

public interface IEventBus
{
    void Publish<T>(T appEvent)
        where T : AppEvent;

    IDisposable Subscribe<T>(Action<T> handler)
        where T : AppEvent;
}
