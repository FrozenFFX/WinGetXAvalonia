// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Diagnostics;
using System.Threading.Tasks;

namespace WinGetXAvalonia.Services.Core;

public sealed class WingetProcess : IWingetProcess
{
    private readonly LoggerService _logger;

    public WingetProcess(LoggerService logger)
    {
        _logger = logger;
    }

    public async Task<string> ExecuteAsync(string arguments)
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

        await process.WaitForExitAsync();

        return output;
    }
}
