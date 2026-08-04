// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using CommunityToolkit.Mvvm.ComponentModel;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Core;
using WinGetXAvalonia.Services.Backup;
using WinGetXAvalonia.Services.Interfaces;

namespace WinGetXAvalonia.ViewModels.Settings;

public abstract partial class SettingsCategoryViewModel : ObservableObject
{
    protected ISettingsService Settings { get; }

    private readonly AutoSaveService _autoSave;

    private bool _isInitializing = true;

    protected bool IsInitializing => _isInitializing;

    public string Title { get; }

    public string Icon { get; }

    protected SettingsCategoryViewModel(
        string title,
        string icon,
        ISettingsService settings,
        AutoSaveService autoSave)
    {
        Title = title;
        Icon = icon;
        Settings = settings;
        _autoSave = autoSave;
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
}
