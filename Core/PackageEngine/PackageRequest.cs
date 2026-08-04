// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

namespace WinGetXAvalonia.Core.PackageEngine;

public class PackageRequest
{
    public string PackageId { get; set; } = "";

    public PackageOperation Operation { get; set; }

    public bool Silent { get; set; }

    public bool Interactive { get; set; }

    public bool Force { get; set; }

    public bool AllowRestart { get; set; }

    public bool IgnoreDependencies { get; set; }
}
