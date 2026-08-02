namespace WinGetXAvalonia.Services;

public class LoggerService
{
    readonly string file = "wingetx.log";

    public void Log(string text)
    {
        File.AppendAllText(
            file,
            $"[{DateTime.Now}] {text}{Environment.NewLine}");
    }
}
