// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WinGetXAvalonia.Core.Providers;
using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Services.Updates;

public sealed class UpdateService : IUpdateService
{
    private readonly IPackageProvider _provider;
    private readonly SemaphoreSlim _gate = new(1, 1);

    private IReadOnlyList<UpdatePackage>? _cache;
    private int _availableCount;

    public UpdateService(IPackageProvider provider)
    {
        _provider = provider;
    }

    public int AvailableCount => _availableCount;

    public event Action? AvailableCountChanged;

    public async Task<IReadOnlyList<UpdatePackage>> GetUpdatesAsync(CancellationToken ct = default)
    {
        if (_cache is not null)
            return _cache;

        await _gate.WaitAsync(ct);

        try
        {
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