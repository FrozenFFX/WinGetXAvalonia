// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using CommunityToolkit.Mvvm.ComponentModel;
using WinGetXAvalonia.ViewModels.Settings;

namespace WinGetXAvalonia.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    public IReadOnlyList<SettingsCategoryViewModel> Categories { get; }

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private SettingsCategoryViewModel? selectedCategory;

    public SettingsViewModel(
        GeneralSettingsViewModel general,
        AppearanceSettingsViewModel appearance,
        PerformanceSettingsViewModel performance,
        WingetSettingsViewModel winget,
        NotificationsSettingsViewModel notifications,
        BackupSettingsViewModel backup,
        DeveloperSettingsViewModel developer,
        AboutSettingsViewModel about)
    {
        Categories = new SettingsCategoryViewModel[]
        {
            general,
            appearance,
            performance,
            winget,
            notifications,
            backup,
            developer,
            about
        };

        SelectedCategory = general;
    }
}
