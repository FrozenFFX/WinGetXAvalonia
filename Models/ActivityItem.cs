// ==========================================================
// WinGetX - https://github.com/FrozenFFX
// ==========================================================
namespace WinGetXAvalonia.Models;

// kaneki
public sealed class ActivityItem
{
    public DateTime Timestamp { get; init; }

    public string Title { get; init; } = "";

    public ActivityType Type { get; init; }

    public string Icon => Type switch
    {
        ActivityType.Install => "📥",
        ActivityType.Update => "🔄",
        ActivityType.Upgrade => "🔄",
        ActivityType.Uninstall => "🗑",
        ActivityType.Error => "⚠️",
        _ => "ℹ️"
    };
}
// kaneki
