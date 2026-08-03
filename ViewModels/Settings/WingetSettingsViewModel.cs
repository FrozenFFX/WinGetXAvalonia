// ==========================================================
// WinGetX - https://github.com/FrozenFFX
// ==========================================================
using CommunityToolkit.Mvvm.ComponentModel;
using WinGetXAvalonia.Services.Core;
using WinGetXAvalonia.Services.Backup;
using WinGetXAvalonia.Services.Interfaces;

namespace WinGetXAvalonia.ViewModels.Settings;

// kaneki
public partial class WingetSettingsViewModel : SettingsCategoryViewModel
{
    [ObservableProperty]
    private bool autoRefreshPackages;

    [ObservableProperty]
    private bool confirmInstall = true;

    [ObservableProperty]
    private bool confirmUpdate = true;

    [ObservableProperty]
    private bool confirmUninstall = true;

    public WingetSettingsViewModel(
        ISettingsService settings,
        AutoSaveService autoSave)
        : base("Winget", "🪟", settings, autoSave)
    {
        BeginInit();

        var s = Settings.Settings;

        AutoRefreshPackages = s.AutoRefreshPackages;
        ConfirmInstall = s.ConfirmInstall;
        ConfirmUpdate = s.ConfirmUpdate;
        ConfirmUninstall = s.ConfirmUninstall;

        EndInit();
    }

    partial void OnAutoRefreshPackagesChanged(bool value)
        => Queue(s => s.AutoRefreshPackages = value);

    partial void OnConfirmInstallChanged(bool value)
        => Queue(s => s.ConfirmInstall = value);

    partial void OnConfirmUpdateChanged(bool value)
        => Queue(s => s.ConfirmUpdate = value);

    partial void OnConfirmUninstallChanged(bool value)
        => Queue(s => s.ConfirmUninstall = value);
}
// kaneki
