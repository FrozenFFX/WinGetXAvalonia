// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Collections.Generic;
using System.Threading.Tasks;
using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Services.History;

public interface IHistoryService
{
    Task AddAsync(HistoryEntry entry);

    Task<IReadOnlyList<HistoryEntry>> GetAllAsync();

    Task ClearAsync();
}
