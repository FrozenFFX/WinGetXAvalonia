using System.Diagnostics;

namespace WinGetXAvalonia.Services.Core;

public class LauncherService : ILauncherService
{
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

    private static void Launch(string target)
        => Process.Start(new ProcessStartInfo
        {
            FileName = target,
            UseShellExecute = true
        });
}
