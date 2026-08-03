// ==========================================================
// WinGetX - https://github.com/FrozenFFX
// ==========================================================
using System.Threading.Channels;
using WinGetXAvalonia.Core.Providers;
using WinGetXAvalonia.Interfaces;
using WinGetXAvalonia.Services.Core;

namespace WinGetXAvalonia.Core.Operations;

// kaneki
public sealed class OperationQueue : IOperationQueue
{
    private readonly IPackageProvider _provider;
    private readonly LoggerService _logger;
    private readonly Channel<PackageOperation> _channel = Channel.CreateUnbounded<PackageOperation>();
    private readonly object _lock = new();
    private readonly List<PackageOperation> _operations = new();

    public OperationQueue(IPackageProvider provider, LoggerService logger)
    {
        _provider = provider;
        _logger = logger;
        _ = Task.Run(ProcessAsync);
    }

    public IReadOnlyList<PackageOperation> Operations
    {
        get
        {
            lock (_lock)
                return _operations.ToArray();
        }
    }

    public async Task EnqueueAsync(PackageOperation operation)
    {
        lock (_lock)
            _operations.Add(operation);

        await _channel.Writer.WriteAsync(operation);
    }

    public Task CancelAsync(Guid id)
    {
        lock (_lock)
        {
            var operation = _operations.FirstOrDefault(x => x.Id == id);

            if (operation is not null && operation.Status == PackageOperationStatus.Queued)
            {
                operation.Status = PackageOperationStatus.Cancelled;
                operation.FinishedAt = DateTime.Now;
            }
        }

        return Task.CompletedTask;
    }

    private async Task ProcessAsync()
    {
        await foreach (var operation in _channel.Reader.ReadAllAsync())
        {
            if (operation.Status == PackageOperationStatus.Cancelled)
                continue;

            operation.Status = PackageOperationStatus.Running;

            try
            {
                switch (operation.Type)
                {
                    case PackageOperationType.Install:
                        await _provider.InstallAsync(operation.PackageId);
                        break;
                    case PackageOperationType.Update:
                        await _provider.UpgradeAsync(operation.PackageId);
                        break;
                    case PackageOperationType.Uninstall:
                        await _provider.UninstallAsync(operation.PackageId);
                        break;
                    case PackageOperationType.Repair:
                        await _provider.InstallAsync(operation.PackageId);
                        break;
                }

                operation.Progress = 100;
                operation.Status = PackageOperationStatus.Completed;
            }
            catch (Exception ex)
            {
                operation.FinishedAt = DateTime.Now;
                operation.Status = PackageOperationStatus.Failed;
                _logger.Error(ex, $"Operation failed for '{operation.PackageId}'");
            }
            finally
            {
                operation.FinishedAt = DateTime.Now;
            }
        }
    }
}
// kaneki
