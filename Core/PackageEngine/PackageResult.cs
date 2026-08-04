namespace WinGetXAvalonia.Core.PackageEngine;

public class PackageResult
{
    public bool Success { get; set; }

    public int ExitCode { get; set; }

    public string Output { get; set; } = "";

    public TimeSpan Duration { get; set; }

    public DateTime Started { get; set; }

    public DateTime Finished { get; set; }
}
