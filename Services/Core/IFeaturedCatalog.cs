using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Services.Core;

public interface IFeaturedCatalog
{
    Task<IReadOnlyList<FeaturedItem>> GetAsync();
}
