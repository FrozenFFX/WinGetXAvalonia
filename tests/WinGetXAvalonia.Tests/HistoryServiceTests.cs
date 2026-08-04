// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Text.Json;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.History;

namespace WinGetXAvalonia.Tests;

public class HistoryServiceTests
{
    private static string CreateTempFile()
    {
        var dir = Path.Combine(
            Path.GetTempPath(),
            "WinGetXAvalonia.Tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, "history.json");
    }

    private static HistoryEntry Entry(string packageId = "Git.Git", bool success = true)
        => new()
        {
            Timestamp = new DateTime(2026, 8, 3, 3, 42, 11),
            Operation = "Install",
            PackageName = "Git",
            PackageId = packageId,
            Version = "2.51.0",
            Success = success
        };

    [Fact]
    public async Task AddAsync_PersistsEntryToDisk()
    {
        var file = CreateTempFile();
        var service = new HistoryService(file);

        await service.AddAsync(Entry());

        var reloaded = new HistoryService(file);
        var entries = await reloaded.GetAllAsync();

        Assert.Single(entries);
        Assert.Equal("Install", entries[0].Operation);
        Assert.Equal("Git", entries[0].PackageName);
        Assert.Equal("Git.Git", entries[0].PackageId);
        Assert.Equal("2.51.0", entries[0].Version);
        Assert.True(entries[0].Success);
        Assert.Equal(new DateTime(2026, 8, 3, 3, 42, 11), entries[0].Timestamp);
    }

    [Fact]
    public async Task AddAsync_MultipleEntries_KeepsOrder()
    {
        var file = CreateTempFile();
        var service = new HistoryService(file);

        await service.AddAsync(Entry(packageId: "Git.Git"));
        await service.AddAsync(Entry(packageId: "Microsoft.PowerShell"));

        var entries = await service.GetAllAsync();

        Assert.Equal(2, entries.Count);
        Assert.Equal("Git.Git", entries[0].PackageId);
        Assert.Equal("Microsoft.PowerShell", entries[1].PackageId);
    }

    [Fact]
    public async Task GetAllAsync_NoFile_ReturnsEmpty()
    {
        var service = new HistoryService(CreateTempFile());

        var entries = await service.GetAllAsync();

        Assert.Empty(entries);
    }

    [Fact]
    public async Task AddAsync_CapturesFailureMessage()
    {
        var file = CreateTempFile();
        var service = new HistoryService(file);

        var entry = Entry(success: false);
        entry.Message = "Exit code: 1";
        await service.AddAsync(entry);

        var reloaded = new HistoryService(file);
        var entries = await reloaded.GetAllAsync();

        Assert.False(entries[0].Success);
        Assert.Equal("Exit code: 1", entries[0].Message);
    }

    [Fact]
    public async Task ClearAsync_RemovesAllEntries()
    {
        var file = CreateTempFile();
        var service = new HistoryService(file);
        await service.AddAsync(Entry());
        await service.AddAsync(Entry());

        await service.ClearAsync();

        var reloaded = new HistoryService(file);
        Assert.Empty(await reloaded.GetAllAsync());
    }

    [Fact]
    public async Task WritesCamelCaseJson()
    {
        var file = CreateTempFile();
        var service = new HistoryService(file);

        await service.AddAsync(Entry());

        string json = await File.ReadAllTextAsync(file);
        using var doc = JsonDocument.Parse(json);
        var element = doc.RootElement[0];

        Assert.True(element.TryGetProperty("packageId", out _));
        Assert.False(element.TryGetProperty("PackageId", out _));
        Assert.False(element.TryGetProperty("message", out _));
    }

    [Fact]
    public async Task GetAllAsync_CorruptFile_ReturnsEmpty()
    {
        var file = CreateTempFile();
        await File.WriteAllTextAsync(file, "not json");

        var service = new HistoryService(file);

        Assert.Empty(await service.GetAllAsync());
    }
}
