using System.Text.Json;
using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Services;

public sealed class SettingsStore
{
    private readonly string _fileName;

    public SettingsStore(string? filePath = null)
    {
        _fileName = filePath ?? "settings.json";
    }

    public AppSettings Settings { get; private set; } = new();

    public void Replace(AppSettings settings)
        => Settings = settings;

    public async Task LoadAsync()
    {
        if (!File.Exists(_fileName))
        {
            await SaveAsync();
            return;
        }

        try
        {
            string json = await File.ReadAllTextAsync(_fileName);

            if (string.IsNullOrWhiteSpace(json))
            {
                Settings = new();
                return;
            }

            Settings =
                JsonSerializer.Deserialize<AppSettings>(json)
                ?? new();
        }
        catch (JsonException)
        {
            Settings = new();
        }
    }

    public async Task SaveAsync()
        => await SaveToAsync(_fileName);

    public async Task SaveToAsync(string filePath)
    {
        string json =
            JsonSerializer.Serialize(
                Settings,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

        await File.WriteAllTextAsync(filePath, json);
    }
}
