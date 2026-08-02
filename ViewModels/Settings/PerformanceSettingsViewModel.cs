using CommunityToolkit.Mvvm.ComponentModel;
using WinGetXAvalonia.Services;
using WinGetXAvalonia.Services.Interfaces;

namespace WinGetXAvalonia.ViewModels.Settings;

public partial class PerformanceSettingsViewModel : SettingsCategoryViewModel
{
    public int MinParallelDownloads => SettingsValidator.MinParallelDownloads;
    public int MaxParallelDownloads => SettingsValidator.MaxParallelDownloads;

    public int MinCacheSize => SettingsValidator.MinCacheSize;
    public int MaxCacheSize => SettingsValidator.MaxCacheSize;

    [ObservableProperty]
    private bool hardwareAcceleration = true;

    [ObservableProperty]
    private bool virtualization = true;

    [ObservableProperty]
    private bool cachePackages = true;

    [ObservableProperty]
    private int parallelDownloads = 4;

    [ObservableProperty]
    private int cacheSize = 512;

    [ObservableProperty]
    private int windowOpacity = 100;

    public PerformanceSettingsViewModel(
        ISettingsService settings,
        AutoSaveService autoSave)
        : base("Performance", "🚀", settings, autoSave)
    {
        BeginInit();

        var s = Settings.Settings;

        HardwareAcceleration = s.HardwareAcceleration;
        Virtualization = s.Virtualization;
        CachePackages = s.CachePackages;
        ParallelDownloads = s.ParallelDownloads;
        CacheSize = s.CacheSize;
        WindowOpacity = s.WindowOpacity != 0 ? s.WindowOpacity : s.Opacity;

        EndInit();
    }

    partial void OnHardwareAccelerationChanged(bool value)
        => Queue(s => s.HardwareAcceleration = value);

    partial void OnVirtualizationChanged(bool value)
        => Queue(s => s.Virtualization = value);

    partial void OnCachePackagesChanged(bool value)
        => Queue(s => s.CachePackages = value);

    partial void OnParallelDownloadsChanged(int value)
        => Queue(s => s.ParallelDownloads = value);

    partial void OnCacheSizeChanged(int value)
        => Queue(s => s.CacheSize = value);

    partial void OnWindowOpacityChanged(int value)
        => Queue(s =>
        {
            s.WindowOpacity = value;
            s.Opacity = value;
        });
}
