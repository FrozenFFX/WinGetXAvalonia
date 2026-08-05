// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Threading.Tasks;
using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Services.Backup;

public interface IBackupService
{
    Task CreateAsync(
        string path,
        BackupModel backup);

    Task<BackupModel> LoadAsync(
        string path);
}
