// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Core;
using WinGetXAvalonia.Services.Interfaces;

namespace WinGetXAvalonia.Services.Backup;

public sealed class AutoSaveService
{
    private readonly ISettingsService _settings;
    private readonly LoggerService? _logger;
    private readonly TimeSpan _delay;
    private CancellationTokenSource? _cts;

    public AutoSaveService(
        ISettingsService settings,
        TimeSpan? delay = null,
        LoggerService? logger = null)
    {
        _settings = settings;
        _delay = delay ?? TimeSpan.FromMilliseconds(500);
        _logger = logger;
    }

    public void Schedule(Action<AppSettings> update)
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();

        SaveLater(update, _cts.Token).FireAndForget(_logger, "Auto-save settings");
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
        catch (Exception ex)
        {
            _logger?.Error(ex, "Failed to auto-save settings");
        }
    }
}
