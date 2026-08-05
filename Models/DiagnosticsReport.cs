// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

namespace WinGetXAvalonia.Models;

public sealed class DiagnosticsReport
{
    public string WindowsVersion { get; init; } = "";

    public string DotNetVersion { get; init; } = "";

    public bool Is64Bit { get; init; }

    public int ProcessorCount { get; init; }

    public string MachineName { get; init; } = "";

    public string UserName { get; init; } = "";

    public bool WingetInstalled { get; init; }
}
