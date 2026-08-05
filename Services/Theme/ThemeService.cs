// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using Avalonia;
using Avalonia.Styling;
using WinGetXAvalonia.Services.Interfaces;

namespace WinGetXAvalonia.Services.Theme;

public sealed class ThemeService : IThemeService
{
    private readonly ISettingsService _settings;

    public ThemeService(ISettingsService settings)
    {
        _settings = settings;
    }

    public ThemeMode CurrentTheme
        => Parse(_settings.Settings.Theme);

    public void ApplyTheme(ThemeMode theme)
    {
        if (Application.Current is not { } app)
            return;

        app.RequestedThemeVariant = theme switch
        {
            ThemeMode.System => ThemeVariant.Default,
            ThemeMode.Light => ThemeVariant.Light,
            ThemeMode.Dark => ThemeVariant.Dark,
            _ => ThemeVariant.Default
        };
    }

    public void ToggleTheme()
    {
        if (Application.Current is not { } app)
            return;

        app.RequestedThemeVariant =
            app.RequestedThemeVariant == ThemeVariant.Dark
                ? ThemeVariant.Light
                : ThemeVariant.Dark;
    }

    private static ThemeMode Parse(string? value)
        => Enum.TryParse<ThemeMode>(value, true, out var theme)
            ? theme
            : ThemeMode.Dark;
}
