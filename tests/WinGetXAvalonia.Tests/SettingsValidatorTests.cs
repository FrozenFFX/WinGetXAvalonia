using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services;

namespace WinGetXAvalonia.Tests;

public class SettingsValidatorTests
{
    private static AppSettings Validate(Action<AppSettings> setup)
    {
        var settings = new AppSettings();
        setup(settings);
        SettingsValidator.Validate(settings);
        return settings;
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 1)]
    [InlineData(16, 16)]
    [InlineData(32, 32)]
    [InlineData(999, 32)]
    public void ParallelDownloads_ClampedToRange(int input, int expected)
    {
        var settings = Validate(s => s.ParallelDownloads = input);
        Assert.Equal(expected, settings.ParallelDownloads);
    }

    [Theory]
    [InlineData(0, 128)]
    [InlineData(128, 128)]
    [InlineData(2048, 2048)]
    [InlineData(8192, 8192)]
    [InlineData(99999, 8192)]
    public void CacheSize_ClampedToRange(int input, int expected)
    {
        var settings = Validate(s => s.CacheSize = input);
        Assert.Equal(expected, settings.CacheSize);
    }

    [Theory]
    [InlineData(-10, 0)]
    [InlineData(0, 0)]
    [InlineData(50, 50)]
    [InlineData(100, 100)]
    [InlineData(150, 100)]
    public void Opacity_ClampedToRange(int input, int expected)
    {
        var settings = Validate(s => s.Opacity = input);
        Assert.Equal(expected, settings.Opacity);
    }

    [Theory]
    [InlineData(0, 60)]
    [InlineData(59, 60)]
    [InlineData(60, 60)]
    [InlineData(75, 75)]
    [InlineData(100, 100)]
    [InlineData(120, 100)]
    public void WindowOpacity_ClampedToRange(int input, int expected)
    {
        var settings = Validate(s => s.WindowOpacity = input);
        Assert.Equal(expected, settings.WindowOpacity);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Theme_EmptyFallsBackToDark(string? input)
    {
        var settings = Validate(s => s.Theme = input!);
        Assert.Equal("Dark", settings.Theme);
    }

    [Fact]
    public void Theme_ValidValuePreserved()
    {
        var settings = Validate(s => s.Theme = "Light");
        Assert.Equal("Light", settings.Theme);
    }

    [Fact]
    public async Task LoadAsync_ClampsInvalidValuesFromFile()
    {
        var dir = Path.Combine(
            Path.GetTempPath(),
            "WinGetXAvalonia.Tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var file = Path.Combine(dir, "settings.json");

        await File.WriteAllTextAsync(
            file,
            """{ "ParallelDownloads": 999, "CacheSize": -100, "Opacity": 500, "WindowOpacity": 5 }""");

        var store = new SettingsStore(file);
        await store.LoadAsync();

        Assert.Equal(32, store.Settings.ParallelDownloads);
        Assert.Equal(128, store.Settings.CacheSize);
        Assert.Equal(100, store.Settings.Opacity);
        Assert.Equal(60, store.Settings.WindowOpacity);
    }
}
