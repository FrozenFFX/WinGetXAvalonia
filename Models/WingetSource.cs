// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

namespace WinGetXAvalonia.Models;

public sealed class WingetSource
{
    public string Name { get; init; } = "";

    public string Argument { get; init; } = "";

    public string Type { get; init; } = "";

    public bool Enabled { get; init; }
}
