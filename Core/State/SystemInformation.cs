// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

namespace WinGetXAvalonia.Core.State;

public sealed record SystemInformation
{
    public string? OperatingSystem { get; set; }

    public string? WindowsVersion { get; set; }

    public string? Architecture { get; set; }

    public string? MachineName { get; set; }

    public string? UserName { get; set; }

    public string? CpuName { get; set; }

    public int? CpuThreads { get; set; }

    public double? RamTotal { get; set; }

    public double? RamUsed { get; set; }

    public double? RamUsage
        => RamTotal.HasValue && RamTotal.Value > 0 && RamUsed.HasValue
            ? Math.Round((RamUsed.Value / RamTotal.Value) * 100, 1)
            : null;

    public string? Disk { get; set; }

    public string? GpuName { get; set; }

    public string? Network { get; set; }

    public string? DotNetVersion { get; set; }

    public string? WingetVersion { get; set; }

    public static SystemInformation Empty => new()
    {
        OperatingSystem = "",
        WindowsVersion = "",
        Architecture = "",
        MachineName = "",
        UserName = "",
        CpuName = "",
        CpuThreads = null,
        RamTotal = null,
        RamUsed = null,
        Disk = "",
        GpuName = "",
        Network = "",
        DotNetVersion = "",
        WingetVersion = ""
    };
}