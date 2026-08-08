// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

namespace WinGetXAvalonia.Models;

public enum RemoteCommandKind
{
    InstallPackage,
    PowerShell,
    Restart
}

public enum RemoteCommandStatus
{
    Pending,
    Success,
    Failed
}

public sealed class RemoteCommand
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string ComputerName { get; set; } = "";

    public RemoteCommandKind Kind { get; set; }

    public string Payload { get; set; } = "";

    public RemoteCommandStatus Status { get; set; } = RemoteCommandStatus.Pending;

    public DateTime Timestamp { get; set; } = DateTime.Now;
}
