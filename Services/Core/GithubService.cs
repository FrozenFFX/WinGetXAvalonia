// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Net.Http;

namespace WinGetXAvalonia.Services.Core;

public class GithubService
{
    readonly HttpClient client = new();

    public async Task<string> Latest()
    {
        return await client.GetStringAsync(
            "https://api.github.com");
    }
}
