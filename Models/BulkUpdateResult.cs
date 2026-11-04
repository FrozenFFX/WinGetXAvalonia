// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

namespace WinGetXAvalonia.Models;

/// <summary>
/// Toplu güncelleme sonucu. Kısmi başarı (fail/cancel) asla toplu
/// "başarı" olarak raporlanmaz.
/// </summary>
public sealed class BulkUpdateResult
{
    public int Succeeded { get; init; }

    public int Failed { get; init; }

    public int CancelledCount { get; init; }

    public bool IsCancelled { get; init; }

    public int Total => Succeeded + Failed + CancelledCount;

    public bool SucceededAll => Total > 0 && Failed == 0 && CancelledCount == 0;

    public static BulkUpdateResult None { get; } = new();
}