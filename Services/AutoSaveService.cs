using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Interfaces;

namespace WinGetXAvalonia.Services;

public sealed class AutoSaveService
{
    private readonly ISettingsService _settings;
    private CancellationTokenSource? _cts;

    public AutoSaveService(ISettingsService settings)
    {
        _settings = settings;
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
            await Task.Delay(500, token);

            await _settings.UpdateAsync(update);
        }
        catch (OperationCanceledException)
        {
        }
    }
}
