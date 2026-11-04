// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using CommunityToolkit.Mvvm.ComponentModel;
using WinGetXAvalonia.Services.Core;
using WinGetXAvalonia.Services.Backup;
using WinGetXAvalonia.Services.Interfaces;
using WinGetXAvalonia.Services.Localization;

namespace WinGetXAvalonia.ViewModels.Settings;

public partial class GeneralSettingsViewModel : SettingsCategoryViewModel
{
    [ObservableProperty]
    private bool startWithWindows;

    [ObservableProperty]
    private bool startMinimized;

    [ObservableProperty]
    private bool minimizeToTray = true;

    [ObservableProperty]
    private bool closeToTray = true;

    [ObservableProperty]
    private bool checkUpdates = true;

    [ObservableProperty]
    private bool showFounderSignature = true;

    [ObservableProperty]
    private string language = "English";

    public IReadOnlyList<string> Languages { get; } = ["English", "Türkçe"];

    private readonly LocalizationManager? _localization;

    public GeneralSettingsViewModel(
        ISettingsService settings,
        AutoSaveService autoSave,
        LocalizationManager? localization = null)
        : base("General", "settings", settings, autoSave)
    {
        _localization = localization;
        BeginInit();

        var s = Settings.Settings;

        StartWithWindows = s.StartWithWindows;
        StartMinimized = s.StartMinimized;
        MinimizeToTray = s.MinimizeToTray;
        CloseToTray = s.CloseToTray;
        CheckUpdates = s.CheckUpdates;
        ShowFounderSignature = s.ShowFounderSignature;
        Language = s.Language == "Turkish" ? "Türkçe" : "English";

        EndInit();
    }

    partial void OnStartWithWindowsChanged(bool value)
        => Queue(s => s.StartWithWindows = value);

    partial void OnStartMinimizedChanged(bool value)
        => Queue(s => s.StartMinimized = value);

    partial void OnMinimizeToTrayChanged(bool value)
        => Queue(s => s.MinimizeToTray = value);

    partial void OnCloseToTrayChanged(bool value)
        => Queue(s => s.CloseToTray = value);

    partial void OnCheckUpdatesChanged(bool value)
        => Queue(s => s.CheckUpdates = value);

    partial void OnShowFounderSignatureChanged(bool value)
        => Queue(s => s.ShowFounderSignature = value);

    partial void OnLanguageChanged(string value)
    {
        var language = value == "Türkçe" ? "Turkish" : "English";
        _localization?.SetLanguage(language);
        Queue(s => s.Language = language);
    }
}
