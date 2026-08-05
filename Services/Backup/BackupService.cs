// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Services.Backup;

public sealed class BackupService : IBackupService
{
    public async Task CreateAsync(
        string path,
        BackupModel backup)
    {
        Directory.CreateDirectory(
            Path.GetDirectoryName(path)!);

        var json = JsonSerializer.Serialize(
            backup,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });

        await File.WriteAllTextAsync(path, json);
    }

    public async Task<BackupModel> LoadAsync(string path)
    {
        var json = await File.ReadAllTextAsync(path);

        return JsonSerializer.Deserialize<BackupModel>(json)!;
    }
}
