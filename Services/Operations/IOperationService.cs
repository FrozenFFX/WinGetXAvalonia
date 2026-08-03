// ==========================================================
// WinGetX - https://github.com/FrozenFFX
// ==========================================================
using System.Threading.Tasks;

namespace WinGetXAvalonia.Services.Operations;

// kaneki
public interface IOperationService
{
    Task InstallAsync(string packageId);

    Task UpdateAsync(string packageId);

    Task RemoveAsync(string packageId);

    Task UpgradeAllAsync();
}
// kaneki
