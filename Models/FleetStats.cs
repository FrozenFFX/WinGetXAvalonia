// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

namespace WinGetXAvalonia.Models;

public sealed class FleetStats
{
    public int Total { get; set; }

    public int Online { get; set; }

    public int Offline { get; set; }

    public int PendingUpdates { get; set; }

    public int CriticalUpdates { get; set; }

    public int FailedInstallations { get; set; }
}
