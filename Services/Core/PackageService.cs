// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WinGetXAvalonia.Core.PackageEngine;
using WinGetXAvalonia.Core.Providers;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Core;
using WinGetXAvalonia.Services.History;
using WinGetXAvalonia.Services.Notifications;
using WinGetXAvalonia.Services.Operations;

namespace WinGetXAvalonia.Services.Core;

public interface IPackageService
{
    Task<PackageOperationResult> InstallAsync(string packageId);
    Task<PackageOperationResult> UpdateAsync(string packageId);
    Task<PackageOperationResult> RemoveAsync(string packageId);
    Task<PackageOperationResult> RepairAsync(string packageId);
    Task<PackageOperationResult> UpgradeAllAsync();
    Task<PackageModel?> GetDetailsAsync(string packageId);
}

public class PackageOperationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public Exception? Error { get; set; }
    public PackageModel? Package { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class PackageService : IPackageService
{
    private readonly IPackageEngine _engine;
    private readonly IPackageProvider _provider;
    private readonly IOperationService _operationService;
    private readonly IHistoryService _historyService;
    private readonly NotificationService _notifications;
    private readonly LoggerService _logger;

    public PackageService(
        IPackageEngine engine,
        IPackageProvider provider,
        IOperationService operationService,
        IHistoryService historyService,
        NotificationService notifications,
        LoggerService logger)
    {
        _engine = engine;
        _provider = provider;
        _operationService = operationService;
        _historyService = historyService;
        _notifications = notifications;
        _logger = logger;
    }

    public async Task<PackageOperationResult> InstallAsync(string packageId)
    {
        try
        {
            await _operationService.InstallAsync(packageId);
            await _historyService.AddAsync(new HistoryEntry
            {
                Timestamp = DateTime.UtcNow,
                Operation = "Install",
                PackageId = packageId,
                Success = true,
                Message = $"Installed {packageId}"
            });
            
            _notifications.Notify(
                NotificationType.Success,
                $"{packageId} Installed",
                $"{packageId} has been installed successfully.");
            
            return new PackageOperationResult
            {
                Success = true,
                Message = $"Installed {packageId}",
                Package = await GetDetailsAsync(packageId)
            };
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Failed to install package: {packageId}");
            
            await _historyService.AddAsync(new HistoryEntry
            {
                Timestamp = DateTime.UtcNow,
                Operation = "Install",
                PackageId = packageId,
                Success = false,
                Message = ex.Message
            });
            
            _notifications.Notify(
                NotificationType.Error,
                $"{packageId} Install Failed",
                $"Unable to install {packageId}. Please try again or check your connection.");
            
            return new PackageOperationResult
            {
                Success = false,
                Message = $"Unable to install {packageId}. Please try again.",
                Error = ex
            };
        }
    }

    public async Task<PackageOperationResult> UpdateAsync(string packageId)
    {
        try
        {
            await _operationService.UpdateAsync(packageId);
            await _historyService.AddAsync(new HistoryEntry
            {
                Timestamp = DateTime.UtcNow,
                Operation = "Update",
                PackageId = packageId,
                Success = true,
                Message = $"Updated {packageId}"
            });
            
            _notifications.Notify(
                NotificationType.Success,
                $"{packageId} Updated",
                $"{packageId} has been updated successfully.");
            
            return new PackageOperationResult
            {
                Success = true,
                Message = $"Updated {packageId}",
                Package = await GetDetailsAsync(packageId)
            };
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Failed to update package: {packageId}");
            
            await _historyService.AddAsync(new HistoryEntry
            {
                Timestamp = DateTime.UtcNow,
                Operation = "Update",
                PackageId = packageId,
                Success = false,
                Message = ex.Message
            });
            
            _notifications.Notify(
                NotificationType.Error,
                $"{packageId} Update Failed",
                $"Unable to update {packageId}. This may be due to a source error or network issue.");
            
            return new PackageOperationResult
            {
                Success = false,
                Message = $"Unable to update {packageId}. Check your connection or try a different source.",
                Error = ex
            };
        }
    }

    public async Task<PackageOperationResult> RemoveAsync(string packageId)
    {
        try
        {
            await _operationService.RemoveAsync(packageId);
            await _historyService.AddAsync(new HistoryEntry
            {
                Timestamp = DateTime.UtcNow,
                Operation = "Remove",
                PackageId = packageId,
                Success = true,
                Message = $"Removed {packageId}"
            });
            
            _notifications.Notify(
                NotificationType.Success,
                $"{packageId} Removed",
                $"{packageId} has been removed successfully.");
            
            return new PackageOperationResult
            {
                Success = true,
                Message = $"Removed {packageId}",
                Package = await GetDetailsAsync(packageId)
            };
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Failed to remove package: {packageId}");
            
            await _historyService.AddAsync(new HistoryEntry
            {
                Timestamp = DateTime.UtcNow,
                Operation = "Remove",
                PackageId = packageId,
                Success = false,
                Message = ex.Message
            });
            
            _notifications.Notify(
                NotificationType.Error,
                $"{packageId} Remove Failed",
                $"Unable to remove {packageId}. Please try again.");
            
            return new PackageOperationResult
            {
                Success = false,
                Message = $"Unable to remove {packageId}. Please try again.",
                Error = ex
            };
        }
    }

    public async Task<PackageOperationResult> RepairAsync(string packageId)
    {
        try
        {
            await _operationService.InstallAsync(packageId);
            await _historyService.AddAsync(new HistoryEntry
            {
                Timestamp = DateTime.UtcNow,
                Operation = "Repair",
                PackageId = packageId,
                Success = true,
                Message = $"Repaired {packageId}"
            });
            
            _notifications.Notify(
                NotificationType.Success,
                $"{packageId} Repaired",
                $"{packageId} has been repaired successfully.");
            
            return new PackageOperationResult
            {
                Success = true,
                Message = $"Repaired {packageId}",
                Package = await GetDetailsAsync(packageId)
            };
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Failed to repair package: {packageId}");
            
            await _historyService.AddAsync(new HistoryEntry
            {
                Timestamp = DateTime.UtcNow,
                Operation = "Repair",
                PackageId = packageId,
                Success = false,
                Message = ex.Message
            });
            
            _notifications.Notify(
                NotificationType.Error,
                $"{packageId} Repair Failed",
                $"Unable to repair {packageId}. Please try again or reinstall.");
            
            return new PackageOperationResult
            {
                Success = false,
                Message = $"Unable to repair {packageId}. Try uninstalling and reinstalling.",
                Error = ex
            };
        }
    }

    public async Task<PackageOperationResult> UpgradeAllAsync()
    {
        try
        {
            await _operationService.UpgradeAllAsync();
            await _historyService.AddAsync(new HistoryEntry
            {
                Timestamp = DateTime.UtcNow,
                Operation = "UpgradeAll",
                Success = true,
                Message = "Upgraded all packages"
            });
            
            _notifications.Notify(
                NotificationType.Success,
                "All Packages Updated",
                "All available packages have been updated successfully.");
            
            return new PackageOperationResult
            {
                Success = true,
                Message = "Upgraded all packages"
            };
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to upgrade all packages");
            
            await _historyService.AddAsync(new HistoryEntry
            {
                Timestamp = DateTime.UtcNow,
                Operation = "UpgradeAll",
                Success = false,
                Message = ex.Message
            });
            
            _notifications.Notify(
                NotificationType.Error,
                "Upgrade All Failed",
                "Unable to update all packages. Check your connection and try again.");
            
            return new PackageOperationResult
            {
                Success = false,
                Message = "Unable to update all packages. Please check your connection.",
                Error = ex
            };
        }
    }

    public async Task<PackageModel?> GetDetailsAsync(string packageId)
    {
        try
        {
            var details = await _provider.GetDetailsAsync(packageId);
            return new PackageModel
            {
                Id = details.Id,
                Name = details.Name,
                Version = details.Version,
                Source = details.Source,
                Description = details.Description
            };
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Failed to get package details: {packageId}");
            return null;
        }
    }
}
