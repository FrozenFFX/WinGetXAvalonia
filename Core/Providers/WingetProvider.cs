// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using WinGetXAvalonia.Core.Operations;
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
    {
        var results = WingetParser.ParseSearch(await _winget.Search(query));

        results = results
            .DistinctBy(x => x.Id)
            .OrderBy(x => x.Name)
            .ToList();

        return results;
    }

    public Task InstallAsync(string id, IOperationProgress? progress = null)
        => _winget.Install(id, progress);

    public Task UpgradeAsync(string id, IOperationProgress? progress = null)
        => _winget.Upgrade(id, progress);

    public Task UninstallAsync(string id, IOperationProgress? progress = null)
        => _winget.Uninstall(id, progress);

    public async Task<IReadOnlyList<InstalledPackage>> GetInstalledAsync()
    {
        var packages = WingetParser.ParseInstalled(await _winget.GetInstalledApps());

        packages = packages
            .OrderBy(x => x.Name)
            .ToList();

        return packages;
    }

    public async Task<IReadOnlyList<UpdatePackage>> GetUpdatesAsync()
    {
        var updates = WingetParser.ParseUpdates(await _winget.GetUpdates());

        updates = updates
            .OrderBy(x => x.Name)
            .ToList();

        return updates;
    }

    public async Task<string?> GetHomepageAsync(string id)
        => WingetParser.ParseHomepage(await _winget.Show(id));

    public async Task<PackageDetails> GetDetailsAsync(string id)
        => WingetParser.ParseDetails(await _winget.Show(id));
}
