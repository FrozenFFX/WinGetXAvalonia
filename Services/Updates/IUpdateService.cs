// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Services.Updates;

public interface IUpdateService
{
    int AvailableCount { get; }

    event Action? AvailableCountChanged;

    Task<IReadOnlyList<UpdatePackage>> GetUpdatesAsync(CancellationToken ct = default);

    void Invalidate();
}