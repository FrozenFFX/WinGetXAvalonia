using CommunityToolkit.Mvvm.ComponentModel;
using WinGetXAvalonia.Services;
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
        Accents = themeManager.Accents;

        BeginInit();

        var s = Settings.Settings;

        SelectedTheme = s.Theme;
        SelectedAccent = Accents.FirstOrDefault(a => a.Name == s.AccentColor) ?? Accents[0];
        EnableAnimations = s.EnableAnimations;
        EnableMica = s.EnableMica || s.Mica;
        EnableAcrylic = s.EnableAcrylic || s.Acrylic;
        RoundedCorners = s.RoundedCorners;

        EndInit();
    }

    partial void OnSelectedThemeChanged(string value)
    {
        if (IsInitializing)
            return;

        _theme.SetTheme(ParseTheme(value));
    }

    partial void OnSelectedAccentChanged(AccentDefinition? value)
    {
        if (IsInitializing || value is null)
            return;

        _themeManager.SetAccent(value);
    }

    partial void OnEnableAnimationsChanged(bool value)
        => Queue(s => s.EnableAnimations = value);

    partial void OnEnableMicaChanged(bool value)
        => Queue(s =>
        {
            s.EnableMica = value;
            s.Mica = value;
        });

    partial void OnEnableAcrylicChanged(bool value)
        => Queue(s =>
        {
            s.EnableAcrylic = value;
            s.Acrylic = value;
        });

    partial void OnRoundedCornersChanged(bool value)
        => Queue(s => s.RoundedCorners = value);

    private static AppTheme ParseTheme(string value)
        => Enum.TryParse<AppTheme>(value, true, out var theme)
            ? theme
            : AppTheme.Dark;
}
