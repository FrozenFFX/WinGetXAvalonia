namespace WinGetXAvalonia.Services.Core;

public class LoggerService
{
    readonly string file = "wingetx.log";

    public void Log(string text)
    {
        File.AppendAllText(
            file,
            $"[{DateTime.Now}] {text}{Environment.NewLine}");
    }

    public void Error(Exception ex, string message)
    {
        Log($"[ERROR] {message}");
        Log($"[ERROR] {ex}");
    }
}
