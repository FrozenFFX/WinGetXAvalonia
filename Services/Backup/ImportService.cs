// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace WinGetXAvalonia.Services.Backup;

public sealed class ImportService : IImportService
{
    public async Task<List<T>> ImportJsonAsync<T>(
        string file)
    {
        var json = await File.ReadAllTextAsync(file);

        return JsonSerializer.Deserialize<List<T>>(json) ?? [];
    }
}
