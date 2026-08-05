// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.IO;
using System.Threading.Tasks;
using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Services.Diagnostics;

public class DiagnosticsService : IDiagnosticsService
{
    public Task<DiagnosticsReport> CreateReportAsync()
    {
        return Task.FromResult(new DiagnosticsReport
        {
            WindowsVersion = Environment.OSVersion.VersionString,
            DotNetVersion = Environment.Version.ToString(),
            Is64Bit = Environment.Is64BitOperatingSystem,
            ProcessorCount = Environment.ProcessorCount,
            MachineName = Environment.MachineName,
            UserName = Environment.UserName,
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
