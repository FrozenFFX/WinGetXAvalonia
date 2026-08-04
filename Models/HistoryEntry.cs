// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System;

namespace WinGetXAvalonia.Models;

public class HistoryEntry
{
    public DateTime Timestamp { get; set; }

    public string Operation { get; set; } = "";

    public string PackageName { get; set; } = "";

    public string PackageId { get; set; } = "";

    public string Version { get; set; } = "";

    public bool Success { get; set; }

    public string? Message { get; set; }
}
