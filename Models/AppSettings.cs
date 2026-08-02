namespace WinGetXAvalonia.Models;

public class AppSettings
{
    public string Theme { get; set; } = "Dark";

    public string AccentColor { get; set; } = "Blue";

    public string Language { get; set; } = "English";

    public bool StartWithWindows { get; set; }

    public bool StartMinimized { get; set; }

    public bool MinimizeToTray { get; set; } = true;

    public bool CloseToTray { get; set; } = true;

    public bool CheckUpdates { get; set; } = true;

    public bool AutoRefreshPackages { get; set; }

    public bool ConfirmInstall { get; set; } = true;

    public bool ConfirmUpdate { get; set; } = true;

    public bool ConfirmUninstall { get; set; } = true;

    public bool CachePackages { get; set; } = true;

    public bool LazyLoading { get; set; } = true;

    public bool Virtualization { get; set; } = true;

    public bool HardwareAcceleration { get; set; } = true;

    public bool EnableAnimations { get; set; } = true;

    public bool Acrylic { get; set; }

    public bool Mica { get; set; }

    public bool EnableAcrylic { get; set; }

    public bool EnableMica { get; set; }

    public bool RoundedCorners { get; set; } = true;

    public bool ToastNotifications { get; set; } = true;

    public bool SoundNotifications { get; set; }

    public bool DeveloperMode { get; set; }

    public bool VerboseLogs { get; set; }

    public int ParallelDownloads { get; set; } = 4;

    public int CacheSize { get; set; } = 512;

    public bool AutoBackup { get; set; }

    public int Opacity { get; set; } = 100;

    public int WindowOpacity { get; set; } = 100;
}
