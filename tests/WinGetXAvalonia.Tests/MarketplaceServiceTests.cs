// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Marketplace;

namespace WinGetXAvalonia.Tests;

public class MarketplaceServiceTests
{
    private static MarketplaceCatalog CreateCatalog()
    {
        return new MarketplaceCatalog
        {
            Version = 1,
            Items =
            [
                new MarketplaceItem
                {
                    Id = "bp-a",
                    Kind = MarketplaceKind.Blueprint,
                    Name = "Dev Setup",
                    Rating = 4.9,
                    RatingCount = 100,
                    Downloads = 5000,
                    Verified = true
                },
                new MarketplaceItem
                {
                    Id = "bp-b",
                    Kind = MarketplaceKind.Blueprint,
                    Name = "Gaming",
                    Rating = 4.0,
                    RatingCount = 10
                },
                new MarketplaceItem
                {
                    Id = "ws-a",
                    Kind = MarketplaceKind.Workspace,
                    Name = "Designer",
                    Rating = 4.5,
                    RatingCount = 20
                },
                new MarketplaceItem
                {
                    Id = "th-a",
                    Kind = MarketplaceKind.Theme,
                    Name = "Solarized",
                    Rating = 4.2,
                    RatingCount = 15
                }
            ]
        };
    }

    [Fact]
    public void GetItems_ReturnsOnlyMatchingKind()
    {
        var service = new MarketplaceService(CreateCatalog());

        var blueprints = service.GetItems(MarketplaceKind.Blueprint);
        var workspaces = service.GetItems(MarketplaceKind.Workspace);
        var themes = service.GetItems(MarketplaceKind.Theme);

        Assert.Equal(2, blueprints.Count);
        Assert.All(blueprints, i => Assert.Equal(MarketplaceKind.Blueprint, i.Kind));
        Assert.Single(workspaces);
        Assert.Single(themes);
    }

    [Fact]
    public void GetItems_OrdersByRatingDescending()
    {
        var service = new MarketplaceService(CreateCatalog());

        var items = service.GetItems(MarketplaceKind.Blueprint);

        Assert.Equal(4.9, items[0].Rating);
        Assert.Equal(4.0, items[1].Rating);
    }

    [Fact]
    public void GetItems_UnknownKind_ReturnsEmpty()
    {
        var service = new MarketplaceService(new MarketplaceCatalog());

        var items = service.GetItems(MarketplaceKind.Theme);

        Assert.Empty(items);
    }

    [Fact]
    public void EmbeddedManifest_LoadsRealCatalog()
    {
        var service = new MarketplaceService();

        Assert.NotEmpty(service.Catalog.Items);
        Assert.Contains(service.Catalog.Items, i => i.Kind == MarketplaceKind.Blueprint);
        Assert.Contains(service.Catalog.Items, i => i.Kind == MarketplaceKind.Workspace);
        Assert.Contains(service.Catalog.Items, i => i.Kind == MarketplaceKind.Theme);
        Assert.Contains(service.Catalog.Items, i => i.Verified);
    }
}
