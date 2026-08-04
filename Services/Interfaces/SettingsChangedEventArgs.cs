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
