// ==========================================================
// WinGetX - https://github.com/FrozenFFX
// ==========================================================
using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Services.Core;

// kaneki
public interface IFeaturedCatalog
{
    Task<IReadOnlyList<FeaturedItem>> GetAsync();
}
// kaneki
