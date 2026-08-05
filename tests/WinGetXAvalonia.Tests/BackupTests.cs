// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Backup;

namespace WinGetXAvalonia.Tests;

public class BackupTests
{
    [Fact]
    public async Task BackupService_CreateThenLoad_RoundTrips()
    {
        var path = TempFile("backup.wgx");
        var service = new BackupService();

        await service.CreateAsync(path, new BackupModel
        {
            Created = new DateTime(2026, 8, 5, 12, 0, 0),
            Workspaces = [new WorkspaceModel { Name = "Dev", Description = "Dev env" }],
            History = [new HistoryEntry { PackageName = "Notepad++", PackageId = "Notepad++.Notepad++" }],
            Settings = [new SettingItem { Name = "Theme", Value = "Dark" }]
        });

        var loaded = await service.LoadAsync(path);

        Assert.Equal(new DateTime(2026, 8, 5, 12, 0, 0), loaded.Created);
        Assert.Single(loaded.Workspaces);
        Assert.Single(loaded.History);
        Assert.Single(loaded.Settings);
        Assert.Empty(loaded.Blueprints);
        Assert.Equal("Dev", loaded.Workspaces[0].Name);
        Assert.Equal("Dark", loaded.Settings[0].Value);

        File.Delete(path);
    }

    [Fact]
    public async Task ExportServiceThenImportService_RoundTrips()
    {
        var path = TempFile("items.json");
        var export = new ExportService();
        var import = new ImportService();

        await export.ExportJsonAsync(
            new[] { new SettingItem { Name = "A", Value = "1" }, new SettingItem { Name = "B", Value = "2" } },
            path);

        var items = await import.ImportJsonAsync<SettingItem>(path);

        Assert.Equal(2, items.Count);
        Assert.Equal("A", items[0].Name);
        Assert.Equal("2", items[1].Value);

        File.Delete(path);
    }

    [Fact]
    public async Task ImportService_NullJson_ReturnsEmptyList()
    {
        var path = TempFile("null.json");
        await File.WriteAllTextAsync(path, "null");
        var import = new ImportService();

        var items = await import.ImportJsonAsync<SettingItem>(path);

        Assert.Empty(items);

        File.Delete(path);
    }

    [Fact]
    public async Task ImportService_EmptyArrayJson_ReturnsEmptyList()
    {
        var path = TempFile("empty.json");
        await File.WriteAllTextAsync(path, "[]");
        var import = new ImportService();

        var items = await import.ImportJsonAsync<SettingItem>(path);

        Assert.Empty(items);

        File.Delete(path);
    }

    [Fact]
    public void BackupModel_Defaults_AreEmpty()
    {
        var backup = new BackupModel();

        Assert.Empty(backup.Workspaces);
        Assert.Empty(backup.History);
        Assert.Empty(backup.Settings);
        Assert.Empty(backup.Blueprints);
    }

    private static string TempFile(string name)
    {
        return Path.Combine(
            Path.GetTempPath(),
            $"wingetx-{Guid.NewGuid():N}-{name}");
    }
}
