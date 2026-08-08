// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Diagnostics;

namespace WinGetXAvalonia.Services.Core;

public class LauncherService : ILauncherService
{
    private readonly LoggerService? _logger;

    public LauncherService(LoggerService? logger = null)
    {
        _logger = logger;
    }

    public Task OpenUrlAsync(string url)
    {
        if (!string.IsNullOrWhiteSpace(url))
            Launch(url);

        return Task.CompletedTask;
    }

    public Task OpenFolderAsync(string path)
    {
        if (!string.IsNullOrWhiteSpace(path))
            Launch(path);

        return Task.CompletedTask;
    }

    public Task OpenFileAsync(string path)
    {
        if (!string.IsNullOrWhiteSpace(path))
            Launch(path);

        return Task.CompletedTask;
    }

    private void Launch(string target)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = target,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, $"Failed to launch '{target}'");
        }
    }
}
