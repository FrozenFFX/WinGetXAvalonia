// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using CommunityToolkit.Mvvm.ComponentModel;
using WinGetXAvalonia.Services.Core;
using WinGetXAvalonia.Services.Backup;
using WinGetXAvalonia.Services.Interfaces;

namespace WinGetXAvalonia.ViewModels.Settings;

public partial class DeveloperSettingsViewModel : SettingsCategoryViewModel
{
    [ObservableProperty]
    private bool developerMode;

    [ObservableProperty]
    private bool verboseLogs;

    public DeveloperSettingsViewModel(
        ISettingsService settings,
        AutoSaveService autoSave)
        : base("Developer", "🛠", settings, autoSave)
    {
        BeginInit();

        var s = Settings.Settings;

        DeveloperMode = s.DeveloperMode;
        VerboseLogs = s.VerboseLogs;

        EndInit();
    }

    partial void OnDeveloperModeChanged(bool value)
        => Queue(s => s.DeveloperMode = value);

    partial void OnVerboseLogsChanged(bool value)
        => Queue(s => s.VerboseLogs = value);
}
