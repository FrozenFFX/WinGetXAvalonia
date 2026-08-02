using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Services.Interfaces;

public interface ISettingsService
{
    AppSettings Settings { get; }

    event EventHandler<SettingsChangedEventArgs>? SettingsChanged;

    Task LoadAsync();

    Task SaveAsync();

    Task UpdateAsync(Action<AppSettings> update);
}
