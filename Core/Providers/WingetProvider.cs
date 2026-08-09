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

    private async Task<T> WithLoadLockAsync<T>(
        Func<Task<T>> action)
    {
        await _loadLock.WaitAsync().ConfigureAwait(false);

        try
        {
            return await action().ConfigureAwait(false);
        }
        finally
        {
            _loadLock.Release();
        }
    }

    private static List<string> GetLinesOrThrow(WingetResult result)
    {
        if (!result.Success)
        {
            throw new InvalidOperationException(
                string.IsNullOrWhiteSpace(result.Error)
                    ? $"winget exited with code {result.ExitCode}."
                    : result.Error);
        }

        return result.Output
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
    }

    public Task<IReadOnlyList<PackageModel>> SearchAsync(string query)
        => WithLoadLockAsync(() => SearchCoreAsync(query));

    private async Task<IReadOnlyList<PackageModel>> SearchCoreAsync(string query)
    {
        var result = await _winget.Search(query);
        var lines = GetLinesOrThrow(result);
        var results = WingetParser.ParseSearch(lines);

        results = results
            .Where(x => !string.IsNullOrWhiteSpace(x.Id))
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
        var result = await _winget.GetInstalledApps();
        var lines = GetLinesOrThrow(result);
        var packages = WingetParser.ParseInstalled(lines);

        packages = packages
            .Where(x => !string.IsNullOrWhiteSpace(x.Id))
            .OrderBy(x => x.Name)
            .ToList();

        return packages;
    }

    public Task<IReadOnlyList<UpdatePackage>> GetUpdatesAsync()
        => WithLoadLockAsync(GetUpdatesCoreAsync);

    private async Task<IReadOnlyList<UpdatePackage>> GetUpdatesCoreAsync()
    {
        var result = await _winget.GetUpdates();
        var lines = GetLinesOrThrow(result);
        var updates = WingetParser.ParseUpdates(lines);

        updates = updates
            .Where(x => !string.IsNullOrWhiteSpace(x.Id))
            .OrderBy(x => x.Name)
            .ToList();

        return updates;
    }

    public Task<string?> GetHomepageAsync(string id)
        => WithLoadLockAsync(() => GetHomepageCoreAsync(id));

    private async Task<string?> GetHomepageCoreAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return null;

        var result = await _winget.Show(id);
        var lines = GetLinesOrThrow(result);
        return WingetParser.ParseHomepage(lines);
    }

    public Task<PackageDetails> GetDetailsAsync(string id)
        => WithLoadLockAsync(() => GetDetailsCoreAsync(id));

    private async Task<PackageDetails> GetDetailsCoreAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return new PackageDetails();

        var result = await _winget.Show(id);
        var lines = GetLinesOrThrow(result);
        return WingetParser.ParseDetails(lines);
    }
}
