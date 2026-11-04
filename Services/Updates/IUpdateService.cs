// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Services.Updates;

public interface IUpdateService
{
    int AvailableCount { get; }

    event Action? AvailableCountChanged;

    /// <summary>
    /// Otoriter mevcut güncelleme listesi. Dashboard, Updates sayfası ve
    /// Sidebar rozeti aynı kaynaktan beslenir.
    /// </summary>
    Task<IReadOnlyList<UpdatePackage>> GetUpdatesAsync(CancellationToken ct = default);

    void Invalidate();

    /// <summary>
    /// Mevcut tüm güncellemeleri (seçimden bağımsız) günceller. Liste, bu
    /// servisin kendi cache'inden üretilir — ikinci bir güncelleme motoru yok.
    /// </summary>
    Task<BulkUpdateResult> UpdateAllAsync(
        IProgress<BulkUpdateProgress>? progress = null,
        CancellationToken ct = default);
}