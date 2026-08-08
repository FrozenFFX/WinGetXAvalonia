// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Blueprints;

namespace WinGetXAvalonia.Tests;

public class BlueprintStoreTests
{
    private static string CreateFolder()
    {
        var dir = Path.Combine(
            Path.GetTempPath(),
            "WinGetXAvalonia.Tests",
            "Blueprints",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        return dir;
    }

    [Fact]
    public async Task SaveAndGetAll_Roundtrips()
    {
        var folder = CreateFolder();
        var store = new BlueprintStore(folder);

        var blueprint = new BlueprintModel
        {
            Name = "Dev Setup",
            Description = "Toolchain",
            Steps = "1. Install Git"
        };

        await store.SaveAsync(blueprint);

        var reloaded = new BlueprintStore(folder);
        var items = await reloaded.GetAllAsync();

        var item = Assert.Single(items);
        Assert.Equal(blueprint.Id, item.Id);
        Assert.Equal("Dev Setup", item.Name);
        Assert.Equal("1. Install Git", item.Steps);
    }

    [Fact]
    public async Task Delete_RemovesBlueprint()
    {
        var folder = CreateFolder();
        var store = new BlueprintStore(folder);

        var blueprint = new BlueprintModel { Name = "Temp" };
        await store.SaveAsync(blueprint);

        await store.DeleteAsync(blueprint.Id);

        var items = await store.GetAllAsync();
        Assert.Empty(items);
    }

    [Fact]
    public async Task ImportJson_AddsFreshIds()
    {
        var folder = CreateFolder();
        var store = new BlueprintStore(folder);

        var original = new BlueprintModel
        {
            Id = Guid.NewGuid(),
            Name = "Imported",
            Steps = "1. Step"
        };

        var imported = await store.ImportJsonAsync(
            System.Text.Json.JsonSerializer.Serialize(original));

        var item = Assert.Single(imported);
        Assert.Equal("Imported", item.Name);
        Assert.NotEqual(original.Id, item.Id);

        var items = await store.GetAllAsync();
        Assert.Single(items);
    }

    [Fact]
    public async Task ImportInvalidJson_Throws()
    {
        var folder = CreateFolder();
        var store = new BlueprintStore(folder);

        await Assert.ThrowsAnyAsync<Exception>(
            () => store.ImportJsonAsync("not-json"));
    }

    [Fact]
    public async Task GetAll_IgnoresCorruptFiles()
    {
        var folder = CreateFolder();
        var store = new BlueprintStore(folder);

        var good = new BlueprintModel { Name = "Good" };
        await store.SaveAsync(good);

        await File.WriteAllTextAsync(
            Path.Combine(folder, $"{Guid.NewGuid()}.blueprint.json"),
            "corrupt{{{");

        var items = await store.GetAllAsync();

        Assert.Single(items);
        Assert.Equal("Good", items[0].Name);
    }
}
