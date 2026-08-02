using Avalonia;
using Avalonia.Styling;
using WinGetXAvalonia.Services.Interfaces;

namespace WinGetXAvalonia.Services.Theme;

public enum AppTheme
{
    System,
    Dark,
    Light
}

public sealed class ThemeService : IThemeService
{
    private readonly ISettingsService _settings;

    public ThemeService(ISettingsService settings)
    {
        _settings = settings;
    }

    public void ApplyCurrent()
        => Apply(Parse(_settings.Settings.Theme));

    public void SetTheme(AppTheme theme)
        => _ = _settings.UpdateAsync(s => s.Theme = theme.ToString());

    internal static void Apply(AppTheme theme)
    {
        if (Application.Current is not { } app)
            return;

        app.RequestedThemeVariant = theme switch
        {
            AppTheme.Dark => ThemeVariant.Dark,
            AppTheme.Light => ThemeVariant.Light,
            _ => ThemeVariant.Default
        };
    }

    internal static AppTheme Parse(string? value)
        => Enum.TryParse<AppTheme>(value, true, out var theme)
            ? theme
            : AppTheme.Dark;
}
