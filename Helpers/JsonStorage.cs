using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace WinGetXAvalonia.Helpers;

public static class JsonStorage
{
    public static async Task Save<T>(string path, T data)
    {
        string json = JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        await File.WriteAllTextAsync(path, json);
    }

    public static async Task<T?> Load<T>(string path)
    {
        if (!File.Exists(path))
            return default;

        return JsonSerializer.Deserialize<T>(
            await File.ReadAllTextAsync(path));
    }
}
