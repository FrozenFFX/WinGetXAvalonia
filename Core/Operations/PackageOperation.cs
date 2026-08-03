// ==========================================================
// WinGetX - https://github.com/FrozenFFX
// ==========================================================
namespace WinGetXAvalonia.Core.Operations;

// kaneki
public sealed class PackageOperation
{
    public PackageOperation(string packageId, PackageOperationType type)
    {
        PackageId = packageId;
        Type = type;
    }

    public Guid Id { get; } = Guid.NewGuid();

    public string PackageId { get; }

    public PackageOperationType Type { get; }

    public PackageOperationStatus Status { get; set; } = PackageOperationStatus.Queued;

    public double Progress { get; set; }

    public DateTime CreatedAt { get; } = DateTime.Now;

    public DateTime? FinishedAt { get; set; }
}
// kaneki
