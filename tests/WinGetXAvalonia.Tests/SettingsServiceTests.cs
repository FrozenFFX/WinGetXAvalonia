// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Core;
using WinGetXAvalonia.Services.Interfaces;

namespace WinGetXAvalonia.Tests;

public class SettingsServiceTests
{
    private static string CreateTempFile()
    {
        var dir = Path.Combine(
            Path.GetTempPath(),
            "WinGetXAvalonia.Tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, "settings.json");
    }

    [Fact]
    public async Task UpdateAsync_PersistsChangesToDisk()
    {
        var file = CreateTempFile();
        var store = new SettingsStore(file);
        var service = new SettingsService(store);

        await service.UpdateAsync(s => s.Theme = "Light");
        await service.UpdateAsync(s => s.ParallelDownloads = 8);

        var reloaded = new SettingsStore(file);
        await reloaded.LoadAsync();

        Assert.Equal("Light", reloaded.Settings.Theme);
        Assert.Equal(8, reloaded.Settings.ParallelDownloads);
    }

    [Fact]
    public async Task SettingsChanged_ReportsOnlyChangedProperties()
    {
        var store = new SettingsStore(CreateTempFile());
        var service = new SettingsService(store);

        IReadOnlyList<string>? changed = null;
        service.SettingsChanged += (_, e) => changed = e.ChangedProperties;

        await service.UpdateAsync(s => s.Theme = "Light");

        Assert.NotNull(changed);
        Assert.Contains(nameof(AppSettings.Theme), changed);
        Assert.DoesNotContain(nameof(AppSettings.AccentColor), changed);
        Assert.Single(changed);
    }

    [Fact]
    public async Task UpdateAsync_DoesNotRaiseEventWhenNothingChanged()
    {
        var store = new SettingsStore(CreateTempFile());
        var service = new SettingsService(store);

        var raised = 0;
        service.SettingsChanged += (_, _) => raised++;

        await service.UpdateAsync(s => s.Theme = s.Theme);

        Assert.Equal(0, raised);
    }

    [Fact]
    public async Task UpdateAsync_ValidatesInvalidValues()
    {
        var store = new SettingsStore(CreateTempFile());
        var service = new SettingsService(store);

        await service.UpdateAsync(s => s.ParallelDownloads = 999);

        Assert.Equal(SettingsValidator.MaxParallelDownloads, store.Settings.ParallelDownloads);
    }

    [Fact]
    public async Task UpdateAsync_ReportsMultipleChangedProperties()
    {
        var store = new SettingsStore(CreateTempFile());
        var service = new SettingsService(store);

        IReadOnlyList<string>? changed = null;
        service.SettingsChanged += (_, e) => changed = e.ChangedProperties;

        await service.UpdateAsync(s =>
        {
            s.Theme = "Light";
            s.AccentColor = "Blue";
        });

        Assert.NotNull(changed);
        Assert.Contains(nameof(AppSettings.Theme), changed);
        Assert.Contains(nameof(AppSettings.AccentColor), changed);
        Assert.Equal(2, changed.Count);
    }

    [Fact]
    public async Task ExportAsync_WritesCurrentSettingsToFile()
    {
        var file = CreateTempFile();
        var service = new SettingsService(new SettingsStore(file));

        await service.UpdateAsync(s => s.Theme = "Light");
        await service.UpdateAsync(s => s.ParallelDownloads = 8);

        var export = Path.Combine(
            Path.GetDirectoryName(file)!,
            "export.json");

        await service.ExportAsync(export);

        var reloaded = new SettingsStore(export);
        await reloaded.LoadAsync();

        Assert.Equal("Light", reloaded.Settings.Theme);
        Assert.Equal(8, reloaded.Settings.ParallelDownloads);
    }

    [Fact]
    public async Task ImportAsync_ReplacesAndPersistsSettings()
    {
        var export = CreateTempFile();
        var exportService = new SettingsService(new SettingsStore(export));
        await exportService.UpdateAsync(s => s.Theme = "Light");
        await exportService.UpdateAsync(s => s.ParallelDownloads = 16);

        var file = CreateTempFile();
        var service = new SettingsService(new SettingsStore(file));
        await service.LoadAsync();

        Assert.Equal("Dark", service.Settings.Theme);

        var ok = await service.ImportAsync(export);

        Assert.True(ok);
        Assert.Equal("Light", service.Settings.Theme);
        Assert.Equal(16, service.Settings.ParallelDownloads);

        var reloaded = new SettingsStore(file);
        await reloaded.LoadAsync();
        Assert.Equal("Light", reloaded.Settings.Theme);
    }

    [Fact]
    public async Task ImportAsync_InvalidFileReturnsFalse()
    {
        var invalid = Path.Combine(
            Path.GetTempPath(),
            "WinGetXAvalonia.Tests",
            Guid.NewGuid().ToString("N"),
            "invalid.json");
        Directory.CreateDirectory(Path.GetDirectoryName(invalid)!);
        await File.WriteAllTextAsync(invalid, "not json");

        var service = new SettingsService(new SettingsStore(CreateTempFile()));

        var ok = await service.ImportAsync(invalid);

        Assert.False(ok);
    }

    [Fact]
    public async Task ImportAsync_RaisesSettingsChanged()
    {
        var export = CreateTempFile();
        var exportService = new SettingsService(new SettingsStore(export));
        await exportService.UpdateAsync(s => s.Theme = "Light");

        var service = new SettingsService(new SettingsStore(CreateTempFile()));

        IReadOnlyList<string>? changed = null;
        service.SettingsChanged += (_, e) => changed = e.ChangedProperties;

        var ok = await service.ImportAsync(export);

        Assert.True(ok);
        Assert.NotNull(changed);
        Assert.Contains(nameof(AppSettings.Theme), changed);
    }

    [Fact]
    public async Task ResetAsync_RestoresDefaultsAndPersists()
    {
        var file = CreateTempFile();
        var service = new SettingsService(new SettingsStore(file));

        await service.UpdateAsync(s =>
        {
            s.Theme = "Light";
            s.ParallelDownloads = 16;
            s.UseMica = true;
        });

        await service.ResetAsync();

        Assert.Equal("Dark", service.Settings.Theme);
        Assert.Equal(4, service.Settings.ParallelDownloads);
        Assert.False(service.Settings.UseMica);

        var reloaded = new SettingsStore(file);
        await reloaded.LoadAsync();
        Assert.Equal("Dark", reloaded.Settings.Theme);
    }
}
