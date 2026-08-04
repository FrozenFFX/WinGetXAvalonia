namespace WinGetXAvalonia.Services.Core;

public interface IClipboardService
{
    Task CopyTextAsync(string text);
}
