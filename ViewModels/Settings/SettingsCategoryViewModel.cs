// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using CommunityToolkit.Mvvm.ComponentModel;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Core;
using WinGetXAvalonia.Services.Backup;
using WinGetXAvalonia.Services.Localization;
using WinGetXAvalonia.Services.Interfaces;

namespace WinGetXAvalonia.ViewModels.Settings;

public abstract partial class SettingsCategoryViewModel : ObservableObject
{
    protected ISettingsService Settings { get; }

    private readonly AutoSaveService _autoSave;

    private bool _isInitializing = true;

    protected bool IsInitializing => _isInitializing;

    private readonly string _title;

    public string Title
        => LocalizationManager.Current?.TranslateValue(_title) ?? _title;

    public string Icon { get; }

    protected SettingsCategoryViewModel(
        string title,
        string icon,
        ISettingsService settings,
        AutoSaveService autoSave)
    {
        _title = title;
        Icon = icon;
        Settings = settings;
        _autoSave = autoSave;
        if (LocalizationManager.Current is { } localization)
            localization.LanguageChanged += OnLanguageChanged;
    }

    protected void BeginInit()
        => _isInitializing = true;

    protected void EndInit()
        => _isInitializing = false;

    protected void Queue(Action<AppSettings> update)
    {
        if (_isInitializing)
            return;

        _autoSave.Schedule(update);
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
        => OnPropertyChanged(nameof(Title));
}
