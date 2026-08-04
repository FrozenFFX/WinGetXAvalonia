// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Runtime.InteropServices;

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
