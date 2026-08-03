// ==========================================================
// WinGetX - https://github.com/FrozenFFX
// ==========================================================
using System.Text.Json;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Interfaces;

namespace WinGetXAvalonia.Services.Core;

// kaneki
public sealed class SettingsService : ISettingsService
{
    private readonly SettingsStore _store;

    public SettingsService(SettingsStore store)
    {
        _store = store;
    }

    public AppSettings Settings => _store.Settings;

    public event EventHandler<SettingsChangedEventArgs>? SettingsChanged;

    public Task LoadAsync() => _store.LoadAsync();

    public Task SaveAsync() => _store.SaveAsync();

    public async Task UpdateAsync(Action<AppSettings> update)
    {
        var before = Snapshot(_store.Settings);

        update(_store.Settings);
        SettingsValidator.Validate(_store.Settings);

        var after = Snapshot(_store.Settings);
        var changed = before
            .Where(pair => !Equals(pair.Value, after[pair.Key]))
            .Select(pair => pair.Key)
            .ToList();

        await _store.SaveAsync();

        if (changed.Count > 0)
            SettingsChanged?.Invoke(
                this,
                new SettingsChangedEventArgs(_store.Settings, changed));
    }

    private static Dictionary<string, object?> Snapshot(AppSettings settings)
        => settings
            .GetType()
            .GetProperties()
            .ToDictionary(p => p.Name, p => p.GetValue(settings));

    public Task ExportAsync(string filePath)
        => _store.SaveToAsync(filePath);

    public async Task<bool> ImportAsync(string filePath)
    {
        if (!File.Exists(filePath))
            return false;

        AppSettings? imported;
        try
        {
            string json = await File.ReadAllTextAsync(filePath);
            imported = JsonSerializer.Deserialize<AppSettings>(json);
        }
        catch (JsonException)
        {
            return false;
        }

        if (imported is null)
            return false;

        var before = Snapshot(_store.Settings);

        _store.Replace(imported);
        SettingsValidator.Validate(_store.Settings);

        var after = Snapshot(_store.Settings);
        var changed = before
            .Where(pair => !Equals(pair.Value, after[pair.Key]))
            .Select(pair => pair.Key)
            .ToList();

        await _store.SaveAsync();

        if (changed.Count > 0)
            SettingsChanged?.Invoke(
                this,
                new SettingsChangedEventArgs(_store.Settings, changed));

        return true;
    }

    public async Task ResetAsync()
    {
        var before = Snapshot(_store.Settings);

        _store.Replace(new AppSettings());
        SettingsValidator.Validate(_store.Settings);

        var after = Snapshot(_store.Settings);
        var changed = before
            .Where(pair => !Equals(pair.Value, after[pair.Key]))
            .Select(pair => pair.Key)
            .ToList();

        await _store.SaveAsync();

        if (changed.Count > 0)
            SettingsChanged?.Invoke(
                this,
                new SettingsChangedEventArgs(_store.Settings, changed));
    }
}
// kaneki
