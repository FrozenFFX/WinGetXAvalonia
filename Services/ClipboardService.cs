using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;

namespace WinGetXAvalonia.Services;

public class ClipboardService
{
    public async Task CopyTextAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        if (Application.Current?.ApplicationLifetime
            is IClassicDesktopStyleApplicationLifetime
            {
                MainWindow: { } window
            } && window.Clipboard is { } clipboard)
        {
            await clipboard.SetTextAsync(text);
        }
    }
}
