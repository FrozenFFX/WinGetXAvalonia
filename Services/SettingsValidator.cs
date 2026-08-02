using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Services;

public static class SettingsValidator
{
    public const int MinParallelDownloads = 1;
    public const int MaxParallelDownloads = 32;

    public const int MinCacheSize = 256;
    public const int MaxCacheSize = 4096;

    public const int MinOpacity = 0;
    public const int MaxOpacity = 100;

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
                MinOpacity,
                MaxOpacity);

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
