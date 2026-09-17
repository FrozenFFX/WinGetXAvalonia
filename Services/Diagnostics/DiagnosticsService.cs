// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Core.State;

namespace WinGetXAvalonia.Services.Diagnostics;

public class DiagnosticsService : IDiagnosticsService
{
    private readonly SystemInformation _system;

    public DiagnosticsService(SystemInformation system)
    {
        _system = system;
    }

    public Task<DiagnosticsReport> CreateReportAsync()
    {
        var ramText = _system.RamTotal.HasValue && _system.RamTotal.Value > 0 && _system.RamUsed.HasValue
            ? $"{_system.RamUsed.Value:0.0} / {_system.RamTotal.Value:0.0} GB"
            : "0.0 % used";

        string? root = Path.GetPathRoot(Environment.SystemDirectory);
        var diskText = "N/A";

        if (root is not null)
        {
            try
            {
                var drive = new DriveInfo(root);
                if (drive.IsReady && drive.TotalSize > 0)
                {
                    double used = drive.TotalSize - drive.AvailableFreeSpace;
                    double usedGb = used / 1024d / 1024d / 1024d;
                    double totalGb = drive.TotalSize / 1024d / 1024d / 1024d;
                    diskText = $"{usedGb:0.0} / {totalGb:0.0} GB";
                }
            }
            catch
            {
                diskText = "N/A";
            }
        }

        return Task.FromResult(new DiagnosticsReport
        {
            WindowsVersion = _system.WindowsVersion ?? Environment.OSVersion.Version.ToString(),
            DotNetVersion = Environment.Version.ToString(),
            Is64Bit = Environment.Is64BitOperatingSystem,
            ProcessorCount = _system.CpuThreads ?? Environment.ProcessorCount,
            CpuName = _system.CpuName ?? "Unknown",
            MachineName = _system.MachineName ?? Environment.MachineName,
            UserName = _system.UserName ?? Environment.UserName,
            Ram = ramText,
            Disk = diskText,
            Architecture = _system.Architecture ?? RuntimeInformation.OSArchitecture.ToString(),
            WingetInstalled = File.Exists(WingetExecutablePath())
        });
    }

    private static double GetRamUsageFallBack()
    {
        MEMORYSTATUSEX status = new();

        if (!GlobalMemoryStatusEx(status))
            return 0;

        double total = status.ullTotalPhys;
        double free = status.ullAvailPhys;

        return Math.Round(((total - free) / total) * 100, 1);
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

    private static string WingetExecutablePath()
    {
        string localAppData = Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData);

        return Path.Combine(localAppData, "Microsoft", "WindowsApps", "winget.exe");
    }
}
