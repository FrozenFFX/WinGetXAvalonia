// ==========================================================
// WinGetX - https://github.com/FrozenFFX
// ==========================================================
using System;
using System.Threading.Tasks;
using WinGetXAvalonia.Core.Providers;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Core;
using WinGetXAvalonia.Services.History;

namespace WinGetXAvalonia.Services.Operations;

// kaneki
public sealed class OperationService : IOperationService
{
    private readonly IPackageProvider _provider;
    private readonly IHistoryService _history;
    private readonly LoggerService _logger;

    public OperationService(IPackageProvider provider, IHistoryService history, LoggerService logger)
    {
        _provider = provider;
        _history = history;
        _logger = logger;
    }

    public async Task InstallAsync(string packageId)
        => await RunAsync("Install", packageId, () => _provider.InstallAsync(packageId));

    public async Task UpdateAsync(string packageId)
        => await RunAsync("Update", packageId, () => _provider.UpgradeAsync(packageId));

    public async Task RemoveAsync(string packageId)
        => await RunAsync("Remove", packageId, () => _provider.UninstallAsync(packageId));

    public async Task UpgradeAllAsync()
    {
        foreach (var update in await _provider.GetUpdatesAsync())
            await RunAsync("Update", update.Id, () => _provider.UpgradeAsync(update.Id));
    }

    private async Task RunAsync(string operation, string packageId, Func<Task> work)
    {
        if (string.IsNullOrWhiteSpace(packageId))
            return;

        bool success = true;
        string? message = null;

        try
        {
            await work();
        }
        catch (Exception ex)
        {
            success = false;
            message = ex.Message;
            _logger.Error(ex, $"Operation '{operation}' failed for '{packageId}'");
        }

        await _history.AddAsync(new HistoryEntry
        {
            Timestamp = DateTime.Now,
            Operation = operation,
            PackageId = packageId,
            Success = success,
            Message = message
        });
    }
}
// kaneki
