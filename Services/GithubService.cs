using System.Net.Http;

namespace WinGetXAvalonia.Services;

public class GithubService
{
    readonly HttpClient client = new();

    public async Task<string> Latest()
    {
        return await client.GetStringAsync(
            "https://api.github.com");
    }
}
