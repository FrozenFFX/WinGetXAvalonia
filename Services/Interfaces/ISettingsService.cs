// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Services.Interfaces;

public interface ISettingsService
{
    AppSettings Settings { get; }

    event EventHandler<SettingsChangedEventArgs>? SettingsChanged;

    Task LoadAsync();

    Task SaveAsync();

    Task UpdateAsync(Action<AppSettings> update);

    Task ExportAsync(string filePath);

    Task<bool> ImportAsync(string filePath);

    Task ResetAsync();
}
