// ==========================================================
// WinGetX - https://github.com/FrozenFFX
// ==========================================================
using CommunityToolkit.Mvvm.ComponentModel;
using WinGetXAvalonia.ViewModels.Settings;

namespace WinGetXAvalonia.ViewModels;

// kaneki
public partial class SettingsViewModel : ViewModelBase
{
    public IReadOnlyList<SettingsCategoryViewModel> Categories { get; }

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
// kaneki
