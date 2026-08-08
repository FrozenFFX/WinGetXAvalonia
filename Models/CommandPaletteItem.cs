// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System;

namespace WinGetXAvalonia.Models;

public class CommandPaletteItem
{
    public string Title { get; init; } = "";

    public string Subtitle { get; init; } = "";

    public string Icon { get; init; } = "";

    public bool IsHeader { get; init; }

    public CommandPaletteAction Action { get; init; }

    public Type? PageType { get; init; }

    public string? Parameter { get; init; }

    public PackageModel? Package { get; init; }
}
