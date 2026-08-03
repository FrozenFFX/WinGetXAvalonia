// ==========================================================
// WinGetX - https://github.com/FrozenFFX
// ==========================================================
using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Services.Core;

// kaneki
public sealed class PackageSearchCache
{
    private readonly object _lock = new();
    private readonly Dictionary<string, List<PackageModel>> _map = new(StringComparer.OrdinalIgnoreCase);

    public bool TryGet(string query, out IReadOnlyList<PackageModel>? results)
    {
        lock (_lock)
        {
            if (_map.TryGetValue(query, out List<PackageModel>? cached))
            {
                results = cached;
                return true;
            }
        }

        results = null;
        return false;
    }

    public void Set(string query, IReadOnlyList<PackageModel> results)
    {
        lock (_lock)
            _map[query] = results.ToList();
    }

    public IReadOnlyList<PackageModel> All()
    {
        lock (_lock)
            return _map.Values.SelectMany(x => x).ToList();
    }

    public void Clear()
    {
        lock (_lock)
            _map.Clear();
    }
}
// kaneki
