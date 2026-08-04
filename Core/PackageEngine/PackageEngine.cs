using WinGetXAvalonia.Services.Operations;

namespace WinGetXAvalonia.Core.PackageEngine;

public class PackageEngine : IPackageEngine
{
    private readonly IOperationService _operations;

    public PackageEngine(IOperationService operations)
    {
        _operations = operations;
    }

    public async Task<PackageResult> ExecuteAsync(PackageRequest request)
    {
        var started = DateTime.Now;

        try
        {
            switch (request.Operation)
            {
                case PackageOperation.Install:
                    await _operations.InstallAsync(request.PackageId);
                    break;

                case PackageOperation.Update:
                    await _operations.UpdateAsync(request.PackageId);
                    break;

                case PackageOperation.Remove:
                    await _operations.RemoveAsync(request.PackageId);
                    break;

                case PackageOperation.UpgradeAll:
                    await _operations.UpgradeAllAsync();
                    break;

                case PackageOperation.Repair:
                default:
                    return new PackageResult
                    {
                        Success = false,
                        Started = started,
                        Finished = DateTime.Now,
                        Duration = DateTime.Now - started,
                        Output = $"Unsupported operation: {request.Operation}"
                    };
            }

            return new PackageResult
            {
                Success = true,
                Started = started,
                Finished = DateTime.Now,
                Duration = DateTime.Now - started
            };
        }
        catch (Exception ex)
        {
            return new PackageResult
            {
                Success = false,
                Started = started,
                Finished = DateTime.Now,
                Duration = DateTime.Now - started,
                Output = ex.Message
            };
        }
    }
}
