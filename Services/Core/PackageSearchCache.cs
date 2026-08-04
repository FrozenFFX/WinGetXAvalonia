// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Services.Core;

public sealed class PackageSearchCache
{
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(5);

    private readonly object _lock = new();
    private readonly TimeSpan _expiration;
    private readonly Dictionary<string, Entry> _map = new(StringComparer.OrdinalIgnoreCase);

    public PackageSearchCache()
        : this(DefaultExpiration)
    {
    }

    public PackageSearchCache(TimeSpan expiration)
        => _expiration = expiration;

    public bool TryGet(string query, out IReadOnlyList<PackageModel>? results)
    {
        lock (_lock)
        {
            if (_map.TryGetValue(query, out var entry))
            {
                if (DateTime.UtcNow - entry.CreatedAt <= _expiration)
                {
                    results = entry.Results;
                    return true;
                }

                _map.Remove(query);
            }
        }

        results = null;
        return false;
    }

    public void Set(string query, IReadOnlyList<PackageModel> results)
    {
        lock (_lock)
            _map[query] = new Entry(results.ToList(), DateTime.UtcNow);
    }

    public IReadOnlyList<PackageModel> All()
    {
        lock (_lock)
            return _map.Values.SelectMany(x => x.Results).ToList();
    }

    public void Clear()
    {
        lock (_lock)
            _map.Clear();
    }

    private sealed record Entry(IReadOnlyList<PackageModel> Results, DateTime CreatedAt);
}
