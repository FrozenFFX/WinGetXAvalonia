using System.Diagnostics;
using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Services;

public class WingetService
{
    private readonly LoggerService _logger;

    public WingetService(LoggerService logger)
    {
        _logger = logger;
    }

    private async Task<List<string>> Execute(string arguments)
    {
        _logger.Log(arguments);

        ProcessStartInfo psi = new()
        {
            FileName = "winget",
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using Process process = Process.Start(psi)!;

        string output = await process.StandardOutput.ReadToEndAsync();

        await Task.Yield();
        await process.WaitForExitAsync();

        return output
            .Split(Environment.NewLine)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();
    }

    public Task<List<string>> GetInstalledApps()
        => Execute("list");

    public Task<List<string>> GetUpdates()
        => Execute("upgrade");

    public Task<List<string>> Search(string query)
        => Execute($"search {query}");

    public Task<List<string>> GetSources()
        => Execute("source list");

    public Task<List<string>> Show(string id)
        => Execute($"show --id {id} --exact --accept-source-agreements");

    public Task Install(string id)
    {
        Process.Start("winget", $"install --id {id} -e");
        return Task.CompletedTask;
    }

    public Task Uninstall(string id)
    {
        Process.Start("winget", $"uninstall --id {id}");
        return Task.CompletedTask;
    }

    public async Task Uninstall(PackageModel package)
    {
        if (string.IsNullOrWhiteSpace(package.Id))
            return;

        Process.Start("winget",
            $"uninstall --id \"{package.Id}\"");

        await Task.CompletedTask;
    }

    public Task Upgrade(string id)
    {
        Process.Start("winget", $"upgrade --id {id}");
        return Task.CompletedTask;
    }

    public Task UpgradeAll()
    {
        Process.Start("winget", "upgrade --all");
        return Task.CompletedTask;
    }

    public Task RefreshSources()
    {
        Process.Start("winget", "source update");
        return Task.CompletedTask;
    }

    public async Task<string> Version()
    {
        ProcessStartInfo psi = new()
        {
            FileName = "winget",
            Arguments = "--version",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using Process p = Process.Start(psi)!;

        return await p.StandardOutput.ReadToEndAsync();
    }
}
