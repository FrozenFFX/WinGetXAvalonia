// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Diagnostics;
using System.Text.RegularExpressions;
using WinGetXAvalonia.Core.Operations;
using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Services.Core;

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

    private async Task<List<string>> RunWithProgress(
        string arguments,
        IOperationProgress? progress,
        string launchingMessage)
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

        progress?.Report(10, launchingMessage);

        var lines = new List<string>();

        while (true)
        {
            string? line = await process.StandardOutput.ReadLineAsync();

            if (line is null)
                break;

            if (string.IsNullOrWhiteSpace(line))
                continue;

            lines.Add(line);

            if (line.Contains('%'))
            {
                var match = Regex.Match(line, @"(\d+)%");

                if (match.Success)
                {
                    progress?.Report(
                        int.Parse(match.Groups[1].Value),
                        line);
                }
            }
        }

        await process.WaitForExitAsync();

        return lines;
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

    public async Task Install(string id, IOperationProgress? progress = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            return;

        progress?.Report(0, "Preparing installation...");

        await RunWithProgress(
            $"install --id {id} -e",
            progress,
            "Launching winget...");

        progress?.Report(100, "Completed");
    }

    public async Task Uninstall(string id, IOperationProgress? progress = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            return;

        progress?.Report(0, "Preparing uninstall...");

        await RunWithProgress(
            $"uninstall --id {id}",
            progress,
            "Launching winget...");

        progress?.Report(100, "Package removed");
    }

    public async Task Uninstall(PackageModel package)
    {
        if (string.IsNullOrWhiteSpace(package.Id))
            return;

        await RunWithProgress(
            $"uninstall --id \"{package.Id}\"",
            null,
            "Launching winget...");
    }

    public async Task Upgrade(string id, IOperationProgress? progress = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            return;

        progress?.Report(0, "Preparing update...");

        await RunWithProgress(
            $"upgrade --id {id}",
            progress,
            "Launching winget...");

        progress?.Report(100, "Update complete");
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
