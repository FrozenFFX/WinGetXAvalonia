// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

namespace WinGetXAvalonia.Models;

public sealed class PolicyViolation
{
    public string ComputerName { get; set; } = "";

    public string RuleName { get; set; } = "";

    public string Package { get; set; } = "";

    public string Message { get; set; } = "";
}
