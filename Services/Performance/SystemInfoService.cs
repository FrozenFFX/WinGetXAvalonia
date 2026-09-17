// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Management;
using System.Runtime.InteropServices;
using WinGetXAvalonia.Core.State;
using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Services.Performance;

public class SystemInfoService
{
    public double GetRamUsage()
    {
        MEMORYSTATUSEX status = new();

        if (!GlobalMemoryStatusEx(status))
            return 0;

        double total = status.ullTotalPhys;
        double free = status.ullAvailPhys;

        return Math.Round(((total - free) / total) * 100, 1);
    }

    public (double TotalGb, double AvailableGb, double UsedGb) GetRamInfo()
    {
        MEMORYSTATUSEX status = new();

        if (!GlobalMemoryStatusEx(status))
            return (0, 0, 0);

        double total = status.ullTotalPhys / (1024.0 * 1024.0 * 1024.0);
        double available = status.ullAvailPhys / (1024.0 * 1024.0 * 1024.0);
        double used = total - available;

        return (Math.Round(total, 1), Math.Round(available, 1), Math.Round(used, 1));
    }

public SystemInformation GetSystemInformation()
    {
        var info = new SystemInformation();

        info.OperatingSystem = Environment.OSVersion.Version.Build >= 22000 ? "Windows 11" : "Windows 10";
        info.WindowsVersion = Environment.OSVersion.Version.ToString();
        info.Architecture = RuntimeInformation.OSArchitecture.ToString();
        info.MachineName = Environment.MachineName;
        info.UserName = Environment.UserName;
        info.CpuName = $"Processor {Environment.ProcessorCount} cores";
        info.CpuThreads = Environment.ProcessorCount;
        var (totalRam, availableRam, usedRam) = GetRamInfo();
        info.RamTotal = Math.Round(totalRam, 1);
        info.RamUsed = Math.Round(usedRam, 1);
        info.Disk = "Not implemented"; // Would require additional WMI queries
        info.GpuName = "Not implemented"; // Would require additional GPU queries
        info.Network = "Not implemented"; // Would require additional network info queries
        info.DotNetVersion = $"{Environment.Version.Major}.{Environment.Version.Minor}";
        info.WingetVersion = "N/A"; // Would call winget version

        return info;
    }

    public IReadOnlyList<HeroInfoItem> GetOsDetails()
    {
        var (totalRam, availableRam, usedRam) = GetRamInfo();
        var ramText = totalRam > 0 ? $"{usedRam:0.0} / {totalRam:0.0} GB" : $"{GetRamUsage():0.0} % used";

        var items = new List<HeroInfoItem>
        {
            new() { Icon = "windows", Label = "OS", Value = GetOsLabel() },
            new() { Icon = "package", Label = "Version", Value = GetVersionLabel() },
            new() { Icon = "tool", Label = "Build", Value = GetBuildLabel() },
            new() { Icon = "enterprise", Label = "Architecture", Value = RuntimeInformation.OSArchitecture.ToString() },
            new() { Icon = "workspace", Label = "Machine", Value = Environment.MachineName },
            new() { Icon = "user", Label = "User", Value = Environment.UserName },
            new() { Icon = "cpu", Label = "CPU", Value = $"{Environment.ProcessorCount} threads" },
            new() { Icon = "settings", Label = "CPU Threads", Value = $"{Environment.ProcessorCount} threads" },
            new() { Icon = "ram", Label = "RAM", Value = ramText },
            new() { Icon = "automation", Label = "GPU", Value = "Not implemented" },
            new() { Icon = "calendar", Label = "Installed", Value = GetInstallDateLabel() },
            new() { Icon = "info", Label = "Edition", Value = GetEditionLabel() }
        };

        return items;
    }

    public static string GetOsLabel()
        => Environment.OSVersion.Version.Build >= 22000 ? "Windows 11" : "Windows 10";

    private static string GetVersionLabel()
    {
        if (!OperatingSystem.IsWindows())
            return Environment.OSVersion.VersionString;

        using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
            @"SOFTWARE\Microsoft\Windows NT\CurrentVersion");

        return key?.GetValue("DisplayVersion") as string
            ?? Environment.OSVersion.VersionString;
    }

    private static string GetBuildLabel()
    {
        if (!OperatingSystem.IsWindows())
            return Environment.OSVersion.Version.Build.ToString();

        using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
            @"SOFTWARE\Microsoft\Windows NT\CurrentVersion");

        string? build = key?.GetValue("CurrentBuildNumber") as string;
        string? ubr = key?.GetValue("UBR")?.ToString();

        if (string.IsNullOrEmpty(build))
            return Environment.OSVersion.Version.Build.ToString();

        return string.IsNullOrEmpty(ubr) ? build : $"{build}.{ubr}";
    }

    private static string GetInstallDateLabel()
    {
        if (!OperatingSystem.IsWindows())
            return "N/A";

        try
        {
            using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows NT\CurrentVersion");

            var raw = key?.GetValue("InstallDate");

            if (raw is long unix)
                return DateTimeOffset.FromUnixTimeSeconds(unix).ToLocalTime().ToString("yyyy-MM-dd");
        }
        catch
        {
            return "N/A";
        }

        return "N/A";
    }

    private static string GetEditionLabel()
    {
        if (!OperatingSystem.IsWindows())
            return "N/A";

        using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
            @"SOFTWARE\Microsoft\Windows NT\CurrentVersion");

        return key?.GetValue("EditionID") as string ?? "N/A";
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private class MEMORYSTATUSEX
    {
        public uint dwLength = (uint)Marshal.SizeOf<MEMORYSTATUSEX>();
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);
}
