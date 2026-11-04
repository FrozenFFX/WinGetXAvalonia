// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

namespace WinGetXAvalonia.Models;

/// <summary>
/// Toplu güncelleme ilerleme raporu. Yüzdeli sahte ilerleme yerine
/// gerçek paket sayıları iletilir (ARTICLE IX / Async).
/// </summary>
public sealed class BulkUpdateProgress
{
    public int Total { get; init; }

    public int Completed { get; init; }

    public int Succeeded { get; init; }

    public int Failed { get; init; }

    public int CancelledCount { get; init; }

    public string CurrentPackage { get; init; } = "";
}