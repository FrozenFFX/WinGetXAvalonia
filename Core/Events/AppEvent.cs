// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

// ==========================================================
// WinGetX - https://github.com/FrozenFFX
// ==========================================================

namespace WinGetXAvalonia.Core.Events;

public abstract class AppEvent
{
    public DateTime Timestamp { get; } = DateTime.Now;
}
