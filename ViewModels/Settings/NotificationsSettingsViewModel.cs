// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using CommunityToolkit.Mvvm.ComponentModel;
using WinGetXAvalonia.Services.Core;
using WinGetXAvalonia.Services.Backup;
using WinGetXAvalonia.Services.Interfaces;

namespace WinGetXAvalonia.ViewModels.Settings;

public partial class NotificationsSettingsViewModel : SettingsCategoryViewModel
{
    [ObservableProperty]
    private bool toastNotifications = true;

    [ObservableProperty]
    private bool soundNotifications;

    public NotificationsSettingsViewModel(
        ISettingsService settings,
        AutoSaveService autoSave)
        : base("Notifications", "notification", settings, autoSave)
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
