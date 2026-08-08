// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using CommunityToolkit.Mvvm.ComponentModel;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Core;
using WinGetXAvalonia.Services.Backup;
using WinGetXAvalonia.Services.Interfaces;
using WinGetXAvalonia.Services.Theme;

namespace WinGetXAvalonia.ViewModels.Settings;

public partial class AppearanceSettingsViewModel : SettingsCategoryViewModel
{
    private readonly IThemeService _theme;
    private readonly ThemeManager _themeManager;

    public IReadOnlyList<AccentDefinition> Accents { get; }

    [ObservableProperty]
    private string selectedTheme = "Dark";

    [ObservableProperty]
    private AccentDefinition? selectedAccent;

    [ObservableProperty]
    private bool enableAnimations = true;

    [ObservableProperty]
    private bool enableMica;

    [ObservableProperty]
    private bool enableAcrylic;

    [ObservableProperty]
    private bool roundedCorners = true;

    public AppearanceSettingsViewModel(
        ISettingsService settings,
        AutoSaveService autoSave,
        ThemeManager themeManager,
        IThemeService theme)
        : base("Appearance", "🎨", settings, autoSave)
    {
        _themeManager = themeManager;
        _theme = theme;
        Accents = themeManager.AllAccents;

        BeginInit();

        var s = Settings.Settings;

        SelectedTheme = s.Theme;
        SelectedAccent = Accents.FirstOrDefault(a => a.Name == s.AccentColor) ?? Accents[0];
        EnableAnimations = s.EnableAnimations;
        EnableMica = s.UseMica;
        EnableAcrylic = s.UseAcrylic;
        RoundedCorners = s.RoundedCorners;

        EndInit();
    }

    partial void OnSelectedThemeChanged(string value)
    {
        if (IsInitializing)
            return;

        _theme.ApplyTheme(ParseTheme(value));
        Queue(s => s.Theme = value);
    }

    partial void OnSelectedAccentChanged(AccentDefinition? value)
    {
        if (IsInitializing || value is null)
            return;

        _themeManager.ApplyAccent(value);
        Queue(s => s.AccentColor = value.Name);
    }

    partial void OnEnableAnimationsChanged(bool value)
        => Queue(s => s.EnableAnimations = value);

    partial void OnEnableMicaChanged(bool value)
        => Queue(s => s.UseMica = value);

    partial void OnEnableAcrylicChanged(bool value)
        => Queue(s => s.UseAcrylic = value);

    partial void OnRoundedCornersChanged(bool value)
        => Queue(s => s.RoundedCorners = value);

    private static ThemeMode ParseTheme(string value)
        => Enum.TryParse<ThemeMode>(value, true, out var theme)
            ? theme
            : ThemeMode.Dark;
}
