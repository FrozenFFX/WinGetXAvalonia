using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Core.Domain.Packages;

public static class PackageStateCalculator
{
    public static PackageState Calculate(PackageModel package, PackageIndex? index)
    {
        if (package is null)
            throw new ArgumentNullException(nameof(package));

        if (index is null || !index.TryGet(package.Id, out var entry) || entry is null)
            return PackageState.NotInstalled;

        var state = new PackageState
        {
            InstalledVersion = entry.InstalledVersion,
            AvailableVersion = entry.AvailableVersion,
            Source = entry.Source
        };

        if (!string.IsNullOrWhiteSpace(entry.InstalledVersion))
            return state with { InstallState = PackageInstallState.Installed };

        return state;
    }

    public static void Apply(PackageModel package, PackageIndex? index)
    {
        if (package is null)
            throw new ArgumentNullException(nameof(package));

        PackageState state = Calculate(package, index);

        package.InstallState = state.InstallState;

        if (!string.IsNullOrWhiteSpace(state.InstalledVersion))
            package.Version = state.InstalledVersion;

        if (!string.IsNullOrWhiteSpace(state.AvailableVersion))
            package.Available = state.AvailableVersion;

        if (string.IsNullOrWhiteSpace(package.Source) && !string.IsNullOrWhiteSpace(state.Source))
            package.Source = state.Source;
    }
}

public sealed record PackageState
{
    public static readonly PackageState NotInstalled = new();

    public PackageInstallState InstallState { get; init; } = PackageInstallState.NotInstalled;

    public string InstalledVersion { get; init; } = "";

    public string AvailableVersion { get; init; } = "";

    public string Source { get; init; } = "";

    public bool HasUpdate => !string.IsNullOrWhiteSpace(AvailableVersion);
}
