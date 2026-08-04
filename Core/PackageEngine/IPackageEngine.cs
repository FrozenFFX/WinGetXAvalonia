namespace WinGetXAvalonia.Core.PackageEngine;

public interface IPackageEngine
{
    Task<PackageResult> ExecuteAsync(PackageRequest request);
}
