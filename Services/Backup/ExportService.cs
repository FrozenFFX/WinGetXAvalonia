// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace WinGetXAvalonia.Services.Backup;

public sealed class ExportService : IExportService
{
    public async Task ExportJsonAsync<T>(
        IEnumerable<T> items,
        string file)
    {
        var json = JsonSerializer.Serialize(
            items,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });

        await File.WriteAllTextAsync(file, json);
    }
}
