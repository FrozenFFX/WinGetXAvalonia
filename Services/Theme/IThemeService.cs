// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

namespace WinGetXAvalonia.Services.Theme;

public interface IThemeService
{
    ThemeMode CurrentTheme { get; }

    void ApplyTheme(ThemeMode theme);
}
