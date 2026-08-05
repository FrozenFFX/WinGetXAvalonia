// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Collections.Generic;
using System.Threading.Tasks;
using WinGetXAvalonia.Helpers;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Core;

namespace WinGetXAvalonia.Services.Sources;

public sealed class SourceManagerService : ISourceManagerService
{
    private readonly IWingetProcess _winget;

    public SourceManagerService(IWingetProcess winget)
    {
        _winget = winget;
    }

    public async Task<IReadOnlyList<WingetSource>> GetSourcesAsync()
    {
        var result = await _winget.ExecuteAsync("source list");

        return WingetSourceParser.Parse(result);
    }

    public Task AddAsync(string name, string arg)
        => _winget.ExecuteAsync($"source add -n {name} {arg}");

    public Task RemoveAsync(string name)
        => _winget.ExecuteAsync($"source remove {name}");

    public Task ResetAsync()
        => _winget.ExecuteAsync("source reset --force");

    public Task UpdateAsync()
        => _winget.ExecuteAsync("source update");
}
