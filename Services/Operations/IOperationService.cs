// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Threading.Tasks;

namespace WinGetXAvalonia.Services.Operations;

public interface IOperationService
{
    Task InstallAsync(string packageId);

    Task UpdateAsync(string packageId);

    Task RemoveAsync(string packageId);

    Task UpgradeAllAsync();
}
