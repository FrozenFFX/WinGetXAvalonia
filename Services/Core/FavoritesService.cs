using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WinGetXAvalonia.Helpers;
using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Services.Core;

public class FavoritesService
{
    private const string FileName = "favorites.json";

    public List<AppPackage> Favorites { get; private set; } = new();

    public async Task Load()
    {
        Favorites = await JsonStorage.Load<List<AppPackage>>(FileName)
            ?? new();
    }

    public async Task Save()
    {
        await JsonStorage.Save(FileName, Favorites);
    }

    public async Task Add(AppPackage package)
    {
        if (Favorites.Any(x => x.Id == package.Id))
            return;

        Favorites.Add(package);
        await Save();
    }

    public async Task Remove(string id)
    {
        Favorites.RemoveAll(x => x.Id == id);
        await Save();
    }
}
