using WinGetXAvalonia.Services;
using WinGetXAvalonia.Services.Interfaces;

namespace WinGetXAvalonia.ViewModels.Settings;

public partial class AboutSettingsViewModel : SettingsCategoryViewModel
{
    public string AppName { get; } = "WinGetX";

    public string Version { get; } = "1.0.0";

    public AboutSettingsViewModel(
        ISettingsService settings,
        AutoSaveService autoSave)
        : base("About", "ℹ", settings, autoSave)
    {
    }
}
