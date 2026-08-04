using WinGetXAvalonia.Core.Providers;
using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Services.PackageDetails;

public class PackageDetailsService : IPackageDetailsService
{
    private readonly IPackageProvider _provider;

    public PackageDetailsService(IPackageProvider provider)
    {
        _provider = provider;
    }

    public async Task<PackageDetailsModel?> GetAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return null;

        var details = await _provider.GetDetailsAsync(id);

        return new PackageDetailsModel
        {
            Name = details.Name,
            Id = details.Id,
            Publisher = details.Publisher,
            Homepage = details.Homepage,
            License = details.License,
            InstalledVersion = details.Version,
            LatestVersion = details.LatestVersion,
            Source = details.Source,
            Description = details.Description
        };
    }
}
