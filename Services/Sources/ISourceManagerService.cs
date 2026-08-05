// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Threading.Tasks;
using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Services.Sources;

public interface ISourceManagerService
{
    Task<IReadOnlyList<WingetSource>> GetSourcesAsync();

    Task AddAsync(string name, string arg);

    Task RemoveAsync(string name);

    Task UpdateAsync();

    Task ResetAsync();
}
