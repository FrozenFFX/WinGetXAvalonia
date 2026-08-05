// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Collections.Generic;
using System.Threading.Tasks;

namespace WinGetXAvalonia.Services.Backup;

public interface IExportService
{
    Task ExportJsonAsync<T>(
        IEnumerable<T> items,
        string file);
}
