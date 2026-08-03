// ==========================================================
// WinGetX - https://github.com/FrozenFFX
// ==========================================================
using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Services.Interfaces;

// kaneki
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
// kaneki
