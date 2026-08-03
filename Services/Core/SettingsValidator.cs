// ==========================================================
// WinGetX - https://github.com/FrozenFFX
// ==========================================================
using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Services.Core;

// kaneki
public static class SettingsValidator
{
    public const int MinParallelDownloads = 1;
    public const int MaxParallelDownloads = 32;

    public const int MinCacheSize = 128;
    public const int MaxCacheSize = 8192;

    public const int MinOpacity = 0;
    public const int MaxOpacity = 100;

    public const int MinWindowOpacity = 60;
    public const int MaxWindowOpacity = 100;

    public static void Validate(AppSettings settings)
    {
        settings.ParallelDownloads =
            Math.Clamp(
                settings.ParallelDownloads,
                MinParallelDownloads,
                MaxParallelDownloads);

        settings.CacheSize =
            Math.Clamp(
                settings.CacheSize,
                MinCacheSize,
                MaxCacheSize);

        settings.WindowOpacity =
            Math.Clamp(
                settings.WindowOpacity,
                MinWindowOpacity,
                MaxWindowOpacity);

        settings.Opacity =
            Math.Clamp(
                settings.Opacity,
                MinOpacity,
                MaxOpacity);

        settings.Theme = Normalize(settings.Theme, "Dark");
        settings.AccentColor = Normalize(settings.AccentColor, "Blue");
        settings.Language = Normalize(settings.Language, "English");
    }

    private static string Normalize(string? value, string fallback)
        => string.IsNullOrWhiteSpace(value) ? fallback : value;
}
// kaneki
