// ==========================================================
// WinGetX - https://github.com/FrozenFFX
// ==========================================================
using System.Windows.Input;

namespace WinGetXAvalonia.Models;

// kaneki
public sealed class QuickActionItem
{
    public string Icon { get; init; } = "";

    public string Title { get; init; } = "";

    public string Description { get; init; } = "";

    public ICommand? Command { get; init; }
}
// kaneki
