// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Runtime.InteropServices;
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

    public string GetWindowsVersion()
    {
        return Environment.OSVersion.VersionString;
    }

    public string GetMachineName()
    {
        return Environment.MachineName;
    }

    public int GetCpuThreads()
    {
        return Environment.ProcessorCount;
    }

    public IReadOnlyList<HeroInfoItem> GetOsDetails()
    {
        var items = new List<HeroInfoItem>
        {
            new() { Icon = "🪟", Label = "OS", Value = GetOsLabel() },
            new() { Icon = "🧩", Label = "Version", Value = GetVersionLabel() },
            new() { Icon = "🔧", Label = "Build", Value = GetBuildLabel() },
            new() { Icon = "🏗", Label = "Architecture", Value = RuntimeInformation.OSArchitecture.ToString() },
            new() { Icon = "💻", Label = "Machine", Value = Environment.MachineName },
            new() { Icon = "👤", Label = "User", Value = Environment.UserName },
            new() { Icon = "🧠", Label = "CPU", Value = $"{Environment.ProcessorCount} threads" },
            new() { Icon = "💾", Label = "RAM", Value = $"{GetRamUsage():0.0} % used" },
            new() { Icon = "📅", Label = "Installed", Value = GetInstallDateLabel() },
            new() { Icon = "🏷", Label = "Edition", Value = GetEditionLabel() }
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
