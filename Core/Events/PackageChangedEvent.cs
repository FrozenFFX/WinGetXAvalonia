// ==========================================================
// WinGetX - https://github.com/FrozenFFX
// ==========================================================

namespace WinGetXAvalonia.Core.Events;

public sealed class PackageChangedEvent : AppEvent
{
    public string PackageId { get; init; } = "";

    public PackageOperation Operation { get; init; }
}

public enum PackageOperation
{
    Installed,
    Updated,
    Removed
}
