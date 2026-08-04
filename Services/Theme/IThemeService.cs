namespace WinGetXAvalonia.Services.Theme;

public interface IThemeService
{
    ThemeMode CurrentTheme { get; }

    void ApplyTheme(ThemeMode theme);
}
