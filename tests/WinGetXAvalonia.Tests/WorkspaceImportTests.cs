// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Text.Json;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Workspaces;

namespace WinGetXAvalonia.Tests;

public class WorkspaceImportTests
{
    private static string CreateFolder()
    {
        var dir = Path.Combine(
            Path.GetTempPath(),
            "WinGetXAvalonia.Tests",
            "Workspaces",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        return dir;
    }

    private static string BuildJson()
    {
        var workspace = new WorkspaceModel
        {
            Id = Guid.NewGuid(),
            Name = "Designer",
            Description = "Design tools"
        };
        workspace.Packages.Add(new WorkspacePackage
        {
            Id = "Figma.Figma",
            Name = "Figma"
        });

        return JsonSerializer.Serialize(workspace);
    }

    [Fact]
    public async Task ImportJson_SavesWorkspaceWithFreshId()
    {
        var folder = CreateFolder();
        var service = new WorkspaceService(folder);

        await service.ImportJsonAsync(BuildJson());

        var items = await service.GetAllAsync();

        var item = Assert.Single(items);
        Assert.Equal("Designer", item.Name);
        Assert.Single(item.Packages);
    }

    [Fact]
    public async Task ImportJson_Malformed_Throws()
    {
        var folder = CreateFolder();
        var service = new WorkspaceService(folder);

        await Assert.ThrowsAsync<InvalidDataException>(
            () => service.ImportJsonAsync("not-json"));
    }

    [Fact]
    public async Task ImportJson_IsListedInGetAll()
    {
        var folder = CreateFolder();
        var service = new WorkspaceService(folder);

        await service.ImportJsonAsync(BuildJson());

        var items = await service.GetAllAsync();

        Assert.NotNull(items.SingleOrDefault(w => w.Name == "Designer"));
    }
}
