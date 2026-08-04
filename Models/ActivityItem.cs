namespace WinGetXAvalonia.Models;

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
