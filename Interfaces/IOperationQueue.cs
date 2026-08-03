// ==========================================================
// WinGetX - https://github.com/FrozenFFX
// ==========================================================
using WinGetXAvalonia.Core.Operations;

namespace WinGetXAvalonia.Interfaces;

// kaneki
public interface IOperationQueue
{
    IReadOnlyList<PackageOperation> Operations { get; }

    Task EnqueueAsync(PackageOperation operation);

    Task CancelAsync(Guid id);
}
// kaneki
