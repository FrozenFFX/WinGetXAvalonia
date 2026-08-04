using WinGetXAvalonia.Core.Domain.Packages;

namespace WinGetXAvalonia.Core.Domain.Package;

public sealed record PackageVersion(string Installed, string Available)
{
    public static readonly PackageVersion None = new("", "");

    public bool IsInstalled => !string.IsNullOrWhiteSpace(Installed);

    public bool HasUpdate => !string.IsNullOrWhiteSpace(Available)
        && PackageVersionComparer.IsNewer(Available, Installed);

    public string InstalledOrDash => IsInstalled ? Installed : "—";
}
