// ==========================================================
// WinGetX - https://github.com/FrozenFFX
// ==========================================================

namespace WinGetXAvalonia.Core.Events;

public abstract class AppEvent
{
    public DateTime Timestamp { get; } = DateTime.Now;
}
