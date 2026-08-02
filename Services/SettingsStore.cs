using System.Text.Json;
using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Services;

public sealed class SettingsStore
{
    const string FileName = "settings.json";

    public AppSettings Settings { get; private set; } = new();

    public async Task LoadAsync()
    {
        if (!File.Exists(FileName))
        {
            await SaveAsync();
            return;
        }

        try
        {
            string json = await File.ReadAllTextAsync(FileName);

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
    {
        string json =
            JsonSerializer.Serialize(
                Settings,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

        await File.WriteAllTextAsync(FileName, json);
    }
}
