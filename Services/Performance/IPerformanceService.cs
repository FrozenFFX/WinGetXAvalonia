// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

namespace WinGetXAvalonia.Services.Performance;

public interface IPerformanceService
{
    PerformanceSnapshot Capture();
}
