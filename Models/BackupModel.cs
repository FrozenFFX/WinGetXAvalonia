// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

namespace WinGetXAvalonia.Models;

public sealed class BackupModel
{
    public DateTime Created { get; init; }

    public List<WorkspaceModel> Workspaces { get; init; } = [];

    public List<HistoryEntry> History { get; init; } = [];

    public List<SettingItem> Settings { get; init; } = [];

    public List<BlueprintModel> Blueprints { get; init; } = [];
}
