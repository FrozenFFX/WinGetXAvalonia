// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using WinGetXAvalonia.Core.Operations;
using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Services.Core;

public class WingetService
{
    private static readonly TimeSpan CommandTimeout = TimeSpan.FromMinutes(2);

    private readonly LoggerService _logger;

    public WingetService(LoggerService logger)
    {
        _logger = logger;
    }

    private async Task<WingetResult> Execute(string arguments)
    {
        using var gate = await WingetExecutionGate.AcquireAsync().ConfigureAwait(false);
        return await ExecuteCoreAsync(arguments + " --disable-interactivity", CommandTimeout)
            .ConfigureAwait(false);
    }

    private async Task<WingetResult> ExecuteCoreAsync(string arguments, TimeSpan timeout)
    {
        _logger.Log(arguments);

        ProcessStartInfo psi = new()
        {
            FileName = "winget",
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        using Process process = Process.Start(psi)!;
        process.EnableRaisingEvents = true;

        using var timeoutCts = new CancellationTokenSource(timeout);

        try
        {
            string output = await process.StandardOutput.ReadToEndAsync(timeoutCts.Token)
                .ConfigureAwait(false);
            string error = await process.StandardError.ReadToEndAsync(timeoutCts.Token)
                .ConfigureAwait(false);

            await process.WaitForExitAsync(timeoutCts.Token).ConfigureAwait(false);

            if (process.ExitCode != 0)
            {
                return WingetResult.Fail(
                    process.ExitCode,
                    string.IsNullOrWhiteSpace(error) ? output.Trim() : error.Trim(),
                    output);
            }

            return WingetResult.Ok(process.ExitCode, output);
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
        {
            try
            {
                if (!process.HasExited)
                    process.Kill(entireProcessTree: true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to terminate timed-out winget process");
            }

            _logger.Error(new TimeoutException($"winget timed out after {timeout.TotalSeconds:0}s"),
                $"Winget command timed out: {arguments}");

            return WingetResult.Fail(-1, $"Winget command timed out after {timeout.TotalSeconds:0} seconds.");
        }
    }

    private async Task<WingetResult> RunWithProgress(
        string arguments,
        IOperationProgress? progress,
        string launchingMessage)
    {
        using var gate = await WingetExecutionGate.AcquireAsync().ConfigureAwait(false);

        _logger.Log(arguments);

        ProcessStartInfo psi = new()
        {
            FileName = "winget",
            Arguments = arguments + " --disable-interactivity",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        using Process process = Process.Start(psi)!;
        process.EnableRaisingEvents = true;

        int activeProgress = 10;

        progress?.Report(activeProgress, launchingMessage);

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
                    activeProgress = int.Parse(match.Groups[1].Value);

                    progress?.Report(
                        activeProgress,
                        line);
                }
            }
        }

        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            string error = await process.StandardError.ReadToEndAsync();

            progress?.Report(activeProgress, error);

            return WingetResult.Fail(
                process.ExitCode,
                error,
                string.Join(Environment.NewLine, lines));
        }

        return WingetResult.Ok(
            process.ExitCode,
            string.Join(Environment.NewLine, lines));
    }

    public async Task<WingetResult> GetInstalledApps()
        => await Execute("list");

    public async Task<WingetResult> GetUpdates()
        => await Execute("upgrade");

    public async Task<WingetResult> Search(string query)
        => await Execute($"search --query \"{query}\" --accept-source-agreements");

    public async Task<WingetResult> GetSources()
        => await Execute("source list");

    public async Task<WingetResult> Show(string id)
        => await Execute($"show --id \"{id}\" --exact --accept-source-agreements");

    public async Task<WingetResult> Install(string id, IOperationProgress? progress = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            return WingetResult.Fail(-1, "Empty package id");

        progress?.Report(0, "Preparing installation...");

        var result = await RunWithProgress(
            $"install --id \"{id}\" --exact --accept-package-agreements --accept-source-agreements",
            progress,
            "Launching winget...");

        if (result.Success)
            progress?.Report(100, "Completed");

        return result;
    }

    public async Task<WingetResult> Uninstall(string id, IOperationProgress? progress = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            return WingetResult.Fail(-1, "Empty package id");

        progress?.Report(0, "Preparing uninstall...");

        var result = await RunWithProgress(
            $"uninstall --id \"{id}\" --exact",
            progress,
            "Launching winget...");

        if (result.Success)
            progress?.Report(100, "Package removed");

        return result;
    }

    public async Task<WingetResult> Uninstall(PackageModel package)
    {
        if (string.IsNullOrWhiteSpace(package.Id))
            return WingetResult.Fail(-1, "Empty package id");

        return await RunWithProgress(
            $"uninstall --id \"{package.Id}\"",
            null,
            "Launching winget...");
    }

    public async Task<WingetResult> Upgrade(string id, IOperationProgress? progress = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            return WingetResult.Fail(-1, "Empty package id");

        progress?.Report(0, "Preparing update...");

        var result = await RunWithProgress(
            $"upgrade --id \"{id}\" --exact --accept-package-agreements --accept-source-agreements",
            progress,
            "Launching winget...");

        if (result.Success)
            progress?.Report(100, "Update complete");

        return result;
    }

    public async Task<WingetResult> UpgradeAll()
    {
        ProcessStartInfo psi = new()
        {
            FileName = "winget",
            Arguments = "upgrade --all --disable-interactivity",
            UseShellExecute = true
        };

        try
        {
            Process.Start(psi);
            return WingetResult.Ok(0, "Started");
        }
        catch (Exception ex)
        {
            return WingetResult.Fail(-1, ex.Message);
        }
    }

    public async Task<WingetResult> RefreshSources()
    {
        ProcessStartInfo psi = new()
        {
            FileName = "winget",
            Arguments = "source update --disable-interactivity",
            UseShellExecute = true
        };

        try
        {
            Process.Start(psi);
            return WingetResult.Ok(0, "Started");
        }
        catch (Exception ex)
        {
            return WingetResult.Fail(-1, ex.Message);
        }
    }

    public async Task<WingetResult> Version()
    {
        ProcessStartInfo psi = new()
        {
            FileName = "winget",
            Arguments = "--version",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        try
        {
            using Process p = Process.Start(psi)!;
            string output = await p.StandardOutput.ReadToEndAsync();
            await p.WaitForExitAsync();
            return WingetResult.Ok(p.ExitCode, output);
        }
        catch (Exception ex)
        {
            return WingetResult.Fail(-1, ex.Message);
        }
    }
}
