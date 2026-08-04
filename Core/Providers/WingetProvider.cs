using WinGetXAvalonia.Helpers;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Core;

namespace WinGetXAvalonia.Core.Providers;

public sealed class WingetProvider : IPackageProvider
{
    private readonly WingetService _winget;

    public WingetProvider(WingetService winget)
    {
        _winget = winget;
    }

    public async Task<IReadOnlyList<PackageModel>> SearchAsync(string query)
        => WingetParser.ParseSearch(await _winget.Search(query));

    public Task InstallAsync(string id)
        => _winget.Install(id);

    public Task UpgradeAsync(string id)
        => _winget.Upgrade(id);

    public Task UninstallAsync(string id)
        => _winget.Uninstall(id);

    public async Task<IReadOnlyList<InstalledPackage>> GetInstalledAsync()
        => WingetParser.ParseInstalled(await _winget.GetInstalledApps());

    public async Task<IReadOnlyList<UpdatePackage>> GetUpdatesAsync()
        => WingetParser.ParseUpdates(await _winget.GetUpdates());

    public async Task<string?> GetHomepageAsync(string id)
        => WingetParser.ParseHomepage(await _winget.Show(id));

    public async Task<PackageDetails> GetDetailsAsync(string id)
        => WingetParser.ParseDetails(await _winget.Show(id));
}
