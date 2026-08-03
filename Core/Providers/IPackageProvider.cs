using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Core.Providers;

public interface IPackageProvider
{
    Task<IReadOnlyList<PackageModel>> SearchAsync(string query);

    Task InstallAsync(string id);

    Task UpgradeAsync(string id);

    Task UninstallAsync(string id);

    Task<IReadOnlyList<InstalledPackage>> GetInstalledAsync();

    Task<IReadOnlyList<UpdatePackage>> GetUpdatesAsync();

    Task<string?> GetHomepageAsync(string id);
}
