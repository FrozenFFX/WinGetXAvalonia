// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

namespace WinGetXAvalonia.Services.Core;

public class LoggerService
{
    private static readonly object FileLock = new();

    private readonly string file = "wingetx.log";

    public void Log(string text)
    {
        lock (FileLock)
        {
            File.AppendAllText(
                file,
                $"[{DateTime.Now}] {text}{Environment.NewLine}");
        }
    }

    public void Error(Exception ex, string message)
    {
        Log($"[ERROR] {message}");
        Log($"[ERROR] {ex}");
    }
}
