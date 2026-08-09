// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Performance;

namespace WinGetXAvalonia.Services.Diagnostics;

public class DiagnosticsService : IDiagnosticsService
{
    private readonly SystemInfoService _system;

    public DiagnosticsService(SystemInfoService system)
    {
        _system = system;
    }

    public Task<DiagnosticsReport> CreateReportAsync()
    {
        var (totalRam, _, usedRam) = _system.GetRamInfo();
        var ramText = totalRam > 0 ? $"{usedRam:0.0} / {totalRam:0.0} GB" : "N/A";

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
            WindowsVersion = _system.GetWindowsVersion(),
            DotNetVersion = Environment.Version.ToString(),
            Is64Bit = Environment.Is64BitOperatingSystem,
            ProcessorCount = Environment.ProcessorCount,
            CpuName = _system.GetCpuModel(),
            MachineName = Environment.MachineName,
            UserName = Environment.UserName,
            Ram = ramText,
            Disk = diskText,
            Architecture = RuntimeInformation.OSArchitecture.ToString(),
            WingetInstalled = File.Exists(WingetExecutablePath())
        });
    }

    private static string WingetExecutablePath()
    {
        string localAppData = Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData);

        return Path.Combine(localAppData, "Microsoft", "WindowsApps", "winget.exe");
    }
}
