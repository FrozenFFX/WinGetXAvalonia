// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Services.Interfaces;

public sealed class SettingsChangedEventArgs : EventArgs
{
    public AppSettings Settings { get; }

    public IReadOnlyList<string> ChangedProperties { get; }

    public SettingsChangedEventArgs(
        AppSettings settings,
        IReadOnlyList<string> changedProperties)
    {
        Settings = settings;
        ChangedProperties = changedProperties;
    }
}
