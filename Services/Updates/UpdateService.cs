// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WinGetXAvalonia.Core.Providers;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Operations;

namespace WinGetXAvalonia.Services.Updates;

public sealed class UpdateService : IUpdateService
{
    private readonly IPackageProvider _provider;
    private readonly IOperationService _operations;
    private readonly SemaphoreSlim _gate = new(1, 1);

    private IReadOnlyList<UpdatePackage>? _cache;
    private int _availableCount;

    public UpdateService(
        IPackageProvider provider,
        IOperationService operations)
    {
        _provider = provider;
        _operations = operations;
    }

    public int AvailableCount => _availableCount;

    public event Action? AvailableCountChanged;

    public async Task<IReadOnlyList<UpdatePackage>> GetUpdatesAsync(CancellationToken ct = default)
    {
        if (_cache is not null)
            return _cache;

        await _gate.WaitAsync(ct).ConfigureAwait(false);

        try
        {
            ct.ThrowIfCancellationRequested();

            if (_cache is not null)
                return _cache;

            var updates = await _provider.GetUpdatesAsync().ConfigureAwait(false);

            _cache = updates ?? Array.Empty<UpdatePackage>();

            PublishCount();

            return _cache;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<BulkUpdateResult> UpdateAllAsync(
        IProgress<BulkUpdateProgress>? progress = null,
        CancellationToken ct = default)
    {
        var updates = await GetUpdatesAsync(ct).ConfigureAwait(false);

        var ids = updates
            .Where(u => !string.IsNullOrWhiteSpace(u.Id))
            .Select(u => u.Id)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (ids.Count == 0)
            return BulkUpdateResult.None;

        return await _operations
            .UpdateAllAsync(ids, progress, ct)
            .ConfigureAwait(false);
    }

    public void Invalidate()
    {
        _cache = null;
    }

    private void PublishCount()
    {
        var count = _cache?.Count ?? 0;

        if (_availableCount == count)
            return;

        _availableCount = count;
        AvailableCountChanged?.Invoke();
    }
}