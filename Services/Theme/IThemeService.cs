// ==========================================================
// WinGetX - https://github.com/FrozenFFX
// ==========================================================
namespace WinGetXAvalonia.Services.Theme;

// kaneki
public interface IThemeService
{
    ThemeMode CurrentTheme { get; }

    void ApplyTheme(ThemeMode theme);
}
// kaneki
