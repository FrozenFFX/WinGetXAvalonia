// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Services.PackageDetails;

public interface IPackageDetailsService
{
    Task<PackageDetailsModel?> GetAsync(string id);
}
