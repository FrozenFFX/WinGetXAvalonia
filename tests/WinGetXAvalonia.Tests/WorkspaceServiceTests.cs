// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Text.Json;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Workspaces;

namespace WinGetXAvalonia.Tests;

public class WorkspaceServiceTests
{
    private static WorkspaceService CreateService(out string folder)
    {
        folder = Path.Combine(Path.GetTempPath(), "WinGetX.Tests", Guid.NewGuid().ToString("N"));
        return new WorkspaceService(folder);
    }

    [Fact]
    public async Task SaveThenGetAll_Roundtrips()
    {
        var service = CreateService(out _);
        var workspace = new WorkspaceModel { Name = "Dev", Description = "Tools", Icon = "💻" };
        workspace.Packages.Add(new WorkspacePackage { Id = "Git.Git", Name = "Git" });
        workspace.Packages.Add(new WorkspacePackage { Id = "Microsoft.VisualStudioCode", Name = "Visual Studio Code" });

        await service.SaveAsync(workspace);

        var all = await service.GetAllAsync();
        var loaded = Assert.Single(all);

        Assert.Equal(workspace.Id, loaded.Id);
        Assert.Equal("Dev", loaded.Name);
        Assert.Equal("Tools", loaded.Description);
        Assert.Equal("💻", loaded.Icon);
        Assert.Equal(2, loaded.PackageCount);
        Assert.Equal(2, loaded.Packages.Count);
    }

    [Fact]
    public async Task GetAll_EmptyFolder_ReturnsEmpty()
    {
        var service = CreateService(out _);

        var all = await service.GetAllAsync();

        Assert.Empty(all);
    }

    [Fact]
    public async Task GetAll_SkipsCorruptFiles()
    {
        var service = CreateService(out var folder);

        await File.WriteAllTextAsync(Path.Combine(folder, "bad.workspace.json"), "not json");

        await service.SaveAsync(new WorkspaceModel { Name = "Dev" });

        var all = await service.GetAllAsync();

        var loaded = Assert.Single(all);
        Assert.Equal("Dev", loaded.Name);
    }

    [Fact]
    public async Task Delete_RemovesWorkspace()
    {
        var service = CreateService(out _);
        var workspace = new WorkspaceModel { Name = "Dev" };

        await service.SaveAsync(workspace);
        await service.DeleteAsync(workspace.Id);

        Assert.Empty(await service.GetAllAsync());
    }

    [Fact]
    public async Task Delete_MissingWorkspace_DoesNotThrow()
    {
        var service = CreateService(out _);

        await service.DeleteAsync(Guid.NewGuid());
    }

    [Fact]
    public async Task Export_WritesWorkspaceToFile()
    {
        var service = CreateService(out _);
        var workspace = new WorkspaceModel { Name = "Dev" };
        await service.SaveAsync(workspace);

        var target = Path.Combine(Path.GetTempPath(), $"wingetx-export-{Guid.NewGuid():N}.json");

        try
        {
            await service.ExportAsync(workspace.Id, target);

            Assert.True(File.Exists(target));

            var exported = JsonSerializer.Deserialize<WorkspaceModel>(await File.ReadAllTextAsync(target));
            Assert.NotNull(exported);
            Assert.Equal("Dev", exported!.Name);
        }
        finally
        {
            if (File.Exists(target))
                File.Delete(target);
        }
    }

    [Fact]
    public async Task Export_MissingWorkspace_Throws()
    {
        var service = CreateService(out _);
        var target = Path.Combine(Path.GetTempPath(), $"wingetx-export-{Guid.NewGuid():N}.json");

        await Assert.ThrowsAsync<FileNotFoundException>(
            () => service.ExportAsync(Guid.NewGuid(), target));
    }

    [Fact]
    public async Task Import_AddsWorkspace_WithFreshId()
    {
        var service = CreateService(out _);
        var source = Path.Combine(Path.GetTempPath(), $"wingetx-import-{Guid.NewGuid():N}.json");
        var workspace = new WorkspaceModel { Name = "Imported" };

        await File.WriteAllTextAsync(
            source,
            JsonSerializer.Serialize(workspace, new JsonSerializerOptions { WriteIndented = true }));

        try
        {
            await service.ImportAsync(source);

            var loaded = Assert.Single(await service.GetAllAsync());
            Assert.Equal("Imported", loaded.Name);
            Assert.NotEqual(workspace.Id, loaded.Id);
        }
        finally
        {
            if (File.Exists(source))
                File.Delete(source);
        }
    }

    [Fact]
    public async Task Import_InvalidFile_Throws()
    {
        var service = CreateService(out _);
        var source = Path.Combine(Path.GetTempPath(), $"wingetx-import-{Guid.NewGuid():N}.json");

        await File.WriteAllTextAsync(source, "not json");

        try
        {
            await Assert.ThrowsAsync<InvalidDataException>(() => service.ImportAsync(source));
        }
        finally
        {
            if (File.Exists(source))
                File.Delete(source);
        }
    }
}
