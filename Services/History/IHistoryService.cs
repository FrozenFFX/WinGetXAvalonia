// ==========================================================
// WinGetX - https://github.com/FrozenFFX
// ==========================================================
using System.Collections.Generic;
using System.Threading.Tasks;
using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Services.History;

// kaneki
public interface IHistoryService
{
    Task AddAsync(HistoryEntry entry);

    Task<IReadOnlyList<HistoryEntry>> GetAllAsync();

    Task ClearAsync();
}
// kaneki
