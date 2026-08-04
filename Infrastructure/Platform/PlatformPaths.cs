namespace WinGetXAvalonia.Infrastructure.Platform;

public sealed class PlatformPaths
{
    public string AppDataDirectory { get; }

    public PlatformPaths(string applicationName = "WinGetX")
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        if (string.IsNullOrWhiteSpace(appData))
            appData = Path.GetTempPath();

        AppDataDirectory = Path.Combine(appData, applicationName);
    }

    public string GetAppDataPath(params string[] paths)
        => Path.Combine(new[] { AppDataDirectory }.Concat(paths).ToArray());
}
