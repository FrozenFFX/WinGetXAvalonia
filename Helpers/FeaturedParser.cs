using System.Text.Json;
using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Helpers;

public static class FeaturedParser
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static IReadOnlyList<FeaturedItem> Parse(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return [];

        try
        {
            return JsonSerializer.Deserialize<List<FeaturedItem>>(json, Options) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
