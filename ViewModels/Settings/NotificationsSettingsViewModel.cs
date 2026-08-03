// ==========================================================
// WinGetX - https://github.com/FrozenFFX
// ==========================================================
using CommunityToolkit.Mvvm.ComponentModel;
using WinGetXAvalonia.Services.Core;
using WinGetXAvalonia.Services.Backup;
using WinGetXAvalonia.Services.Interfaces;

namespace WinGetXAvalonia.ViewModels.Settings;

// kaneki
public partial class NotificationsSettingsViewModel : SettingsCategoryViewModel
{
    [ObservableProperty]
    private bool toastNotifications = true;

    [ObservableProperty]
    private bool soundNotifications;

    public NotificationsSettingsViewModel(
        ISettingsService settings,
        AutoSaveService autoSave)
        : base("Notifications", "🔔", settings, autoSave)
    {
        BeginInit();

        var s = Settings.Settings;

        ToastNotifications = s.ToastNotifications;
        SoundNotifications = s.SoundNotifications;

        EndInit();
    }

    partial void OnToastNotificationsChanged(bool value)
        => Queue(s => s.ToastNotifications = value);

    partial void OnSoundNotificationsChanged(bool value)
        => Queue(s => s.SoundNotifications = value);
}
// kaneki
