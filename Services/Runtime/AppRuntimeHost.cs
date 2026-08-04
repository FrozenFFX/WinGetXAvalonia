// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using WinGetXAvalonia.Core.Runtime;
using WinGetXAvalonia.Services.Interfaces;
using WinGetXAvalonia.Services.Theme;

namespace WinGetXAvalonia.Services.Runtime;

public sealed class AppRuntimeHost : RuntimeHost
{
    private readonly ISettingsService _settings;
    private readonly ThemeManager _theme;

    public AppRuntimeHost(
        RuntimeContext context,
        RuntimeFeatureManager features,
        ISettingsService settings,
        ThemeManager theme)
        : base(context, features)
    {
        _settings = settings;
        _theme = theme;
    }

    protected override async Task LoadConfigurationAsync()
        => await _settings.LoadAsync();

    protected override Task LoadThemesAsync()
    {
        _theme.ApplyCurrent();
        return Task.CompletedTask;
    }
}
