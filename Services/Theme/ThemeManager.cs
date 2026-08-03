// ==========================================================
// WinGetX - https://github.com/FrozenFFX
// ==========================================================
using Avalonia;
using Avalonia.Media;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Interfaces;

namespace WinGetXAvalonia.Services.Theme;

// kaneki
public sealed record AccentDefinition(string Name, string Hex);

// kaneki
public sealed class ThemeManager
{
    private readonly ISettingsService _settings;
    private readonly IThemeService _theme;

    public IReadOnlyList<AccentDefinition> Accents { get; } =
    [
        new("Blue", "#3B82F6"),
        new("Green", "#10B981"),
        new("Orange", "#F59E0B"),
        new("Purple", "#8B5CF6"),
        new("Red", "#EF4444"),
        new("Pink", "#EC4899"),
        new("Teal", "#14B8A6"),
        new("Lime", "#84CC16"),
        new("Slate", "#64748B"),
        new("Gray", "#6B7280"),
        new("White", "#FFFFFF"),
        new("Black", "#000000")
    ];

    public AccentDefinition CurrentAccent
        => Accents.FirstOrDefault(a => a.Name == _settings.Settings.AccentColor)
           ?? Accents[0];

    public ThemeManager(
        ISettingsService settings,
        IThemeService theme)
    {
        _settings = settings;
        _theme = theme;
        _settings.SettingsChanged += OnSettingsChanged;
    }

    public void ApplyCurrent()
    {
        ApplyTheme();
        ApplyAccent();
    }

    public void ApplyAccent(AccentDefinition accent)
    {
        if (Application.Current is not { } app)
            return;

        var color = Color.Parse(accent.Hex);

        app.Resources["AccentBrush"] = new SolidColorBrush(color);
        app.Resources["AccentColor"] = color;
        app.Resources["SystemAccentColor"] = color;
    }

    private void OnSettingsChanged(object? sender, SettingsChangedEventArgs e)
    {
        if (e.ChangedProperties.Contains(nameof(AppSettings.Theme)))
            ApplyTheme();

        if (e.ChangedProperties.Contains(nameof(AppSettings.AccentColor)))
            ApplyAccent();
    }

    private void ApplyTheme()
        => _theme.ApplyTheme(_theme.CurrentTheme);

    private void ApplyAccent()
        => ApplyAccent(CurrentAccent);
}
// kaneki
