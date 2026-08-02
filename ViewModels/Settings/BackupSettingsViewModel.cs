using CommunityToolkit.Mvvm.ComponentModel;
using WinGetXAvalonia.Services;
using WinGetXAvalonia.Services.Interfaces;

namespace WinGetXAvalonia.ViewModels.Settings;

public partial class BackupSettingsViewModel : SettingsCategoryViewModel
{
    [ObservableProperty]
    private bool autoBackup;

    public BackupSettingsViewModel(
        ISettingsService settings,
        AutoSaveService autoSave)
        : base("Backup", "💾", settings, autoSave)
    {
        BeginInit();

        AutoBackup = Settings.Settings.AutoBackup;

        EndInit();
    }

    partial void OnAutoBackupChanged(bool value)
        => Queue(s => s.AutoBackup = value);
}
