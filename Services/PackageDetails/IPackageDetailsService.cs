using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Services.PackageDetails;

public interface IPackageDetailsService
{
    Task<PackageDetailsModel?> GetAsync(string id);
}
