// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

namespace WinGetXAvalonia.Models;

public sealed class ActivityItem
{
    public DateTime Timestamp { get; init; }

    public string Title { get; init; } = "";

    public ActivityType Type { get; init; }

    public string Icon => Type switch
    {
        ActivityType.Install => "install",
        ActivityType.Update => "update",
        ActivityType.Upgrade => "update",
        ActivityType.Uninstall => "uninstall",
        ActivityType.Error => "warning",
        _ => "info"
    };
}
