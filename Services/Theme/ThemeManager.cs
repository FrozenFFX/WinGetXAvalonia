// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using Avalonia;
using Avalonia.Media;
using Avalonia.Threading;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Interfaces;

namespace WinGetXAvalonia.Services.Theme;

public sealed class ThemeManager
{
    private readonly ISettingsService _settings;
    private readonly IThemeService _theme;

    public IReadOnlyList<AccentDefinition> Accents { get; } =
    [
        new("Blue", "#3B82F6"),
        new("Green", "#15803D"),
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

    private readonly List<AccentDefinition> _customAccents = new();

    public IReadOnlyList<AccentDefinition> AllAccents
        => Accents.Concat(_customAccents).ToList();

    public AccentDefinition CurrentAccent
        => AllAccents.FirstOrDefault(a => a.Name == _settings.Settings.AccentColor)
           ?? AllAccents.FirstOrDefault(a => a.Hex == _settings.Settings.AccentColor)
           ?? Accents[0];

    public ThemeManager(
        ISettingsService settings,
        IThemeService theme)
    {
        _settings = settings;
        _theme = theme;
        _settings.SettingsChanged += OnSettingsChanged;

        var installed = _settings.Settings.InstalledAccents;

        if (installed is not null)
        {
            foreach (var accent in installed)
                RegisterAccent(accent);
        }
    }

    public void ApplyCurrent()
    {
        ApplyTheme();
        ApplyAccent();
    }

    public void RegisterAccent(AccentDefinition accent)
    {
        if (string.IsNullOrWhiteSpace(accent.Name) || string.IsNullOrWhiteSpace(accent.Hex))
            return;

        if (_customAccents.Any(a => a.Name == accent.Name))
            return;

        _customAccents.Add(accent);
    }

    public async Task InstallAccent(AccentDefinition accent)
    {
        RegisterAccent(accent);

        var installed = _settings.Settings.InstalledAccents;

        if (installed is not null && installed.All(a => a.Name != accent.Name))
        {
            installed.Add(accent);
            await _settings.SaveAsync();
        }

        ApplyAccent(accent);
    }

    public void ApplyAccent(AccentDefinition accent)
    {
        void Apply()
        {
            if (Application.Current is not { } app)
                return;

            var color = Color.Parse(accent.Hex);

            app.Resources["AccentBrush"] = new SolidColorBrush(color);
            app.Resources["AccentColor"] = color;
            app.Resources["SystemAccentColor"] = color;
        }

        if (Application.Current is null)
            return;

        if (Dispatcher.UIThread.CheckAccess())
            Apply();
        else
            Dispatcher.UIThread.Post(Apply);
    }

    private void OnSettingsChanged(object? sender, SettingsChangedEventArgs e)
    {
        void Apply()
        {
            if (e.ChangedProperties.Contains(nameof(AppSettings.Theme)))
                ApplyTheme();

            if (e.ChangedProperties.Contains(nameof(AppSettings.AccentColor)))
                ApplyAccent();
        }

        if (Dispatcher.UIThread.CheckAccess())
            Apply();
        else
            Dispatcher.UIThread.Post(Apply);
    }

    private void ApplyTheme()
        => _theme.ApplyTheme(_theme.CurrentTheme);

    private void ApplyAccent()
        => ApplyAccent(CurrentAccent);
}
