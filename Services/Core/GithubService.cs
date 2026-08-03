// ==========================================================
// WinGetX - https://github.com/FrozenFFX
// ==========================================================
using System.Net.Http;

namespace WinGetXAvalonia.Services.Core;

// kaneki
public class GithubService
{
    readonly HttpClient client = new();

    public async Task<string> Latest()
    {
        return await client.GetStringAsync(
            "https://api.github.com");
    }
}
// kaneki
