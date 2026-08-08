// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

namespace WinGetXAvalonia.Models;

public enum PolicyAction
{
    BlockUninstall,
    BlockInstall,
    AllowOnly
}

public sealed class PolicyRule
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = "";

    public PolicyAction Action { get; set; }

    public string Package { get; set; } = "";

    public bool Enabled { get; set; } = true;

    public string Description { get; set; } = "";
}
