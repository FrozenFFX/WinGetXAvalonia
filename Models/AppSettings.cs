// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

namespace WinGetXAvalonia.Models;

public class AppSettings
{
    public string Theme { get; set; } = "Dark";

    public string AccentColor { get; set; } = "Blue";

    public List<AccentDefinition> InstalledAccents { get; set; } = new();

    public string Language { get; set; } = "English";

    public List<string> FavoriteStudioTools { get; set; } = new();

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

    public bool UseAcrylic { get; set; }

    public bool UseMica { get; set; }

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

    public bool ShowFounderSignature { get; set; } = true;
}
