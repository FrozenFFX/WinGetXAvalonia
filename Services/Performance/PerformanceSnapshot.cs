// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

namespace WinGetXAvalonia.Services.Performance;

public sealed class PerformanceSnapshot
{
    public long Memory { get; init; }

    public int Threads { get; init; }

    public int Handles { get; init; }

    public TimeSpan CpuTime { get; init; }
}
