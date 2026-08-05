// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Diagnostics;

namespace WinGetXAvalonia.Services.Performance;

public sealed class PerformanceService : IPerformanceService
{
    private readonly Process _process = Process.GetCurrentProcess();

    public PerformanceSnapshot Capture()
    {
        _process.Refresh();

        return new PerformanceSnapshot
        {
            Memory = _process.WorkingSet64,
            Threads = _process.Threads.Count,
            Handles = _process.HandleCount,
            CpuTime = _process.TotalProcessorTime
        };
    }
}
