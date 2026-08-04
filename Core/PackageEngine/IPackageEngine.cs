// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

namespace WinGetXAvalonia.Core.PackageEngine;

public interface IPackageEngine
{
    Task<PackageResult> ExecuteAsync(PackageRequest request);
}
