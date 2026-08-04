using WinGetXAvalonia.Services.Backup;
using WinGetXAvalonia.Services.Core;
using WinGetXAvalonia.Services.Theme;
using WinGetXAvalonia.ViewModels.Settings;

namespace WinGetXAvalonia.Tests;

public class AppearanceSettingsViewModelTests
{
    private static (string File, SettingsService Settings, AppearanceSettingsViewModel Vm) Create()
    {
        var dir = Path.Combine(
            Path.GetTempPath(),
            "WinGetXAvalonia.Tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var file = Path.Combine(dir, "settings.json");

        var settings = new SettingsService(new SettingsStore(file));
        var theme = new ThemeService(settings);
        var autoSave = new AutoSaveService(settings, TimeSpan.FromMilliseconds(20));
        var manager = new ThemeManager(settings, theme);
        var vm = new AppearanceSettingsViewModel(settings, autoSave, manager, theme);

        return (file, settings, vm);
    }

    [Fact]
    public async Task ThemeChange_AppliesAndPersistsAfterDebounce()
    {
        var (file, _, vm) = Create();

        vm.SelectedTheme = "Light";
        await Task.Delay(200);

        var reloaded = new SettingsStore(file);
        await reloaded.LoadAsync();

        Assert.Equal("Light", reloaded.Settings.Theme);
    }

    [Fact]
    public async Task AccentChange_AppliesAndPersistsAfterDebounce()
    {
        var (file, _, vm) = Create();

        vm.SelectedAccent = vm.Accents.First(a => a.Name == "Green");
        await Task.Delay(200);

        var reloaded = new SettingsStore(file);
        await reloaded.LoadAsync();

        Assert.Equal("Green", reloaded.Settings.AccentColor);
    }

    [Fact]
    public async Task InitialValues_AreLoadedFromSettings()
    {
        var dir = Path.Combine(
            Path.GetTempPath(),
            "WinGetXAvalonia.Tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var file = Path.Combine(dir, "settings.json");

        var settings = new SettingsService(new SettingsStore(file));
        await settings.UpdateAsync(s => s.Theme = "Light");

        var reloaded = new SettingsService(new SettingsStore(file));
        await reloaded.LoadAsync();

        var theme = new ThemeService(reloaded);
        var autoSave = new AutoSaveService(reloaded, TimeSpan.FromMilliseconds(20));
        var manager = new ThemeManager(reloaded, theme);
        var vm = new AppearanceSettingsViewModel(reloaded, autoSave, manager, theme);

        Assert.Equal("Light", vm.SelectedTheme);
    }
}
