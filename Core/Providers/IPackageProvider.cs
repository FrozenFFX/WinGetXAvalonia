// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using WinGetXAvalonia.Core.Operations;
using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Core.Providers;

public interface IPackageProvider
{
    Task<IReadOnlyList<PackageModel>> SearchAsync(string query);

    Task InstallAsync(string id, IOperationProgress? progress = null);

    Task UpgradeAsync(string id, IOperationProgress? progress = null);

    Task UninstallAsync(string id, IOperationProgress? progress = null);

    Task<IReadOnlyList<InstalledPackage>> GetInstalledAsync();

    Task<IReadOnlyList<UpdatePackage>> GetUpdatesAsync();

    Task<string?> GetHomepageAsync(string id);

    Task<PackageDetails> GetDetailsAsync(string id);
}
