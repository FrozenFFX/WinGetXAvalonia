// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Text.Json;
using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Services.Marketplace;

public sealed class MarketplaceService
{
    private const string ManifestResource = "avares://WinGetXAvalonia/Assets/marketplace/manifest.json";

    private readonly MarketplaceCatalog _catalog;

    public MarketplaceService()
        : this(null)
    {
    }

    public MarketplaceService(MarketplaceCatalog? catalog)
    {
        _catalog = catalog ?? LoadCatalog();
    }

    public MarketplaceCatalog Catalog => _catalog;

    public IReadOnlyList<MarketplaceItem> GetItems(MarketplaceKind kind)
        => _catalog.Items
            .Where(i => i.Kind == kind)
            .OrderByDescending(i => i.Rating)
            .ToList();

    public bool IsInstalled(MarketplaceItem item)
        => item.IsInstalled;

    private static MarketplaceCatalog LoadCatalog()
    {
        try
        {
            var uri = new Uri(ManifestResource);

            using var stream = Avalonia.Platform.AssetLoader.Open(uri);

            if (stream is null)
                return CreateFallbackCatalog();

            var catalog = JsonSerializer.Deserialize<MarketplaceCatalog>(
                new StreamReader(stream).ReadToEnd());

            if (catalog is not null && catalog.Items.Count > 0)
                return catalog;
        }
        catch (Exception)
        {
            // kaynak yoksa fallback katalog kullanılır
        }

        return CreateFallbackCatalog();
    }

    private static MarketplaceCatalog CreateFallbackCatalog()
    {
        return new MarketplaceCatalog
        {
            Version = 1,
            Items =
            [
                new MarketplaceItem
                {
                    Id = "bp-git-vscode",
                    Kind = MarketplaceKind.Blueprint,
                    Name = "Git & VS Code",
                    Description = "Core version control and editor setup.",
                    Author = "FrozenFFX",
                    Category = "Developer",
                    Icon = "tool",
                    Rating = 4.5,
                    RatingCount = 12,
                    Verified = true,
                    Downloads = 500,
                    Payload = "{\"Name\":\"Git & VS Code\",\"Description\":\"Core dev setup.\",\"Steps\":\"1. Install Git\\n2. Install VS Code\"}"
                }
            ]
        };
    }
}
