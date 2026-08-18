// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Diagnostics;

namespace WinGetXAvalonia.Services.Core;

public sealed class WingetProcess : IWingetProcess
{
    private static readonly TimeSpan CommandTimeout = TimeSpan.FromMinutes(2);

    private readonly LoggerService _logger;

    public WingetProcess(LoggerService logger)
    {
        _logger = logger;
    }

    public async Task<string> ExecuteAsync(string arguments)
    {
        using var gate = await WingetExecutionGate.AcquireAsync().ConfigureAwait(false);

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
        using var timeoutCts = new CancellationTokenSource(CommandTimeout);

        try
        {
            string output = await process.StandardOutput.ReadToEndAsync(timeoutCts.Token)
                .ConfigureAwait(false);

            await process.WaitForExitAsync(timeoutCts.Token).ConfigureAwait(false);

            return output;
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

            _logger.Error(new TimeoutException($"winget timed out after {CommandTimeout.TotalSeconds:0}s"),
                $"Winget command timed out: {arguments}");

            throw new TimeoutException(
                $"Winget command timed out after {CommandTimeout.TotalSeconds:0} seconds.");
        }
    }
}
