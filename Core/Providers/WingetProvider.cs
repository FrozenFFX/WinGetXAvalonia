// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using WinGetXAvalonia.Core.Operations;
using WinGetXAvalonia.Helpers;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Core;

namespace WinGetXAvalonia.Core.Providers;

public sealed class WingetProvider : IPackageProvider
{
    private readonly SemaphoreSlim _loadLock = new(1, 1);
    private readonly WingetService _winget;

    public WingetProvider(WingetService winget)
    {
        _winget = winget;
    }

    private async Task<T> WithLoadLockAsync<T>(Func<Task<T>> action)
    {
        await _loadLock.WaitAsync();

        try
        {
            return await action();
        }
        finally
        {
            _loadLock.Release();
        }
    }

    public Task<IReadOnlyList<PackageModel>> SearchAsync(string query)
        => WithLoadLockAsync(() => SearchCoreAsync(query));

    private async Task<IReadOnlyList<PackageModel>> SearchCoreAsync(string query)
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

    public Task<IReadOnlyList<InstalledPackage>> GetInstalledAsync()
        => WithLoadLockAsync(GetInstalledCoreAsync);

    private async Task<IReadOnlyList<InstalledPackage>> GetInstalledCoreAsync()
    {
        var packages = WingetParser.ParseInstalled(await _winget.GetInstalledApps());

        packages = packages
            .OrderBy(x => x.Name)
            .ToList();

        return packages;
    }

    public Task<IReadOnlyList<UpdatePackage>> GetUpdatesAsync()
        => WithLoadLockAsync(GetUpdatesCoreAsync);

    private async Task<IReadOnlyList<UpdatePackage>> GetUpdatesCoreAsync()
    {
        var updates = WingetParser.ParseUpdates(await _winget.GetUpdates());

        updates = updates
            .OrderBy(x => x.Name)
            .ToList();

        return updates;
    }

    public Task<string?> GetHomepageAsync(string id)
        => WithLoadLockAsync(() => GetHomepageCoreAsync(id));

    private async Task<string?> GetHomepageCoreAsync(string id)
        => WingetParser.ParseHomepage(await _winget.Show(id));

    public Task<PackageDetails> GetDetailsAsync(string id)
        => WithLoadLockAsync(() => GetDetailsCoreAsync(id));

    private async Task<PackageDetails> GetDetailsCoreAsync(string id)
        => WingetParser.ParseDetails(await _winget.Show(id));
}
