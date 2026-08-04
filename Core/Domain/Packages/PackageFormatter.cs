using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Core.Domain.Packages;

public static class PackageFormatter
{
    public static string InstallButtonText(PackageInstallState state, bool hasUpdate) => state switch
    {
        PackageInstallState.NotInstalled => "Install",
        PackageInstallState.Installing => "Installing...",
        PackageInstallState.Updating => "Updating...",
        PackageInstallState.Installed when hasUpdate => "Update",
        _ => "Installed"
    };

    public static string InstalledVersion(string? version)
        => $"Installed {version}";

    public static string AvailableVersion(string? version)
        => $"Available {version}";

    public static string UpdateMarker(string? version)
        => $"▲ {version}";

    public static string Version(string? version)
        => $"Version {version}";

    public static string NotInstalledText
        => "Not Installed";

    public static string ValueOrDash(string? value)
        => string.IsNullOrWhiteSpace(value) ? "-" : value;
}
