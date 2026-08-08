// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using Avalonia;
using Avalonia.Styling;
using Avalonia.Threading;
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
        void Apply()
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

        if (Application.Current is null)
            return;

        if (Dispatcher.UIThread.CheckAccess())
            Apply();
        else
            Dispatcher.UIThread.Post(Apply);
    }

    public void ToggleTheme()
    {
        void Toggle()
        {
            if (Application.Current is not { } app)
                return;

            app.RequestedThemeVariant =
                app.RequestedThemeVariant == ThemeVariant.Dark
                    ? ThemeVariant.Light
                    : ThemeVariant.Dark;
        }

        if (Application.Current is null)
            return;

        if (Dispatcher.UIThread.CheckAccess())
            Toggle();
        else
            Dispatcher.UIThread.Post(Toggle);
    }

    private static ThemeMode Parse(string? value)
        => Enum.TryParse<ThemeMode>(value, true, out var theme)
            ? theme
            : ThemeMode.Dark;
}
