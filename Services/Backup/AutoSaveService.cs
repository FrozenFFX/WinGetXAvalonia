// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Interfaces;

namespace WinGetXAvalonia.Services.Backup;

public sealed class AutoSaveService
{
    private readonly ISettingsService _settings;
    private readonly TimeSpan _delay;
    private CancellationTokenSource? _cts;

    public AutoSaveService(ISettingsService settings, TimeSpan? delay = null)
    {
        _settings = settings;
        _delay = delay ?? TimeSpan.FromMilliseconds(500);
    }

    public void Schedule(Action<AppSettings> update)
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();

        _ = SaveLater(update, _cts.Token);
    }

    private async Task SaveLater(
        Action<AppSettings> update,
        CancellationToken token)
    {
        try
        {
            await Task.Delay(_delay, token);

            await _settings.UpdateAsync(update);
        }
        catch (OperationCanceledException)
        {
        }
    }
}
