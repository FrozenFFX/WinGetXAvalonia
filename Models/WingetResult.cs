// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

namespace WinGetXAvalonia.Models;

public record WingetResult
{
    public bool Success { get; init; }
    public int ExitCode { get; init; }
    public string Output { get; init; } = "";
    public string Error { get; init; } = "";

    public static WingetResult Ok(int exitCode, string output) =>
        new() { Success = true, ExitCode = exitCode, Output = output };

    public static WingetResult Fail(int exitCode, string error, string output = "") =>
        new() { Success = false, ExitCode = exitCode, Error = error, Output = output };
}