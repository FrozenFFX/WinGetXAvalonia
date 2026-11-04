// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Services.Operations;

public interface IOperationService
{
    Task InstallAsync(string packageId);

    Task UpdateAsync(string packageId);

    Task RemoveAsync(string packageId);

    /// <summary>
    /// Yalnızca verilen paket ID'lerini günceller (Update Selected için alt uç
    /// nokta). Dış çağrıcı (ViewModel), güncellenecek liste tek kaynak olan
    /// IUpdateService üzerinden üretir.
    /// </summary>
    Task<BulkUpdateResult> UpdateAllAsync(
        IReadOnlyList<string> packageIds,
        IProgress<BulkUpdateProgress>? progress = null,
        CancellationToken ct = default);

    /// <summary>
    /// Uyumluluk / otomasyon yolu: provider üzerinden gelen tüm güncellemeleri
    /// toplu günceller. UI yolları (Dashboard, Updates) bunu değil, otoriter
    /// IUpdateService.UpdateAllAsync kullanır.
    /// </summary>
    Task<BulkUpdateResult> UpgradeAllAsync(
        IProgress<BulkUpdateProgress>? progress = null,
        CancellationToken ct = default);
}