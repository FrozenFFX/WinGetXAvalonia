// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Threading;
using System.Threading.Tasks;
using WinGetXAvalonia.Core.Providers;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Notifications;

namespace WinGetXAvalonia.Services.Core;

public interface IBackgroundIndexer
{
    Task<IReadOnlyList<InstalledPackage>> GetInstalledPackagesAsync();
    Task<IReadOnlyList<UpdatePackage>> GetAvailableUpdatesAsync();
    Task<WinGetXAvalonia.Models.PackageDetails?> GetPackageDetailsAsync(string packageId);
    Task<IReadOnlyList<PackageModel>> SearchAsync(string query);
    Task RefreshAsync();
    event EventHandler<string>? ProgressChanged;
    event EventHandler<string>? StatusChanged;
}

public class BackgroundIndexer : IBackgroundIndexer
{
    private readonly IPackageProvider _provider;
    private readonly NotificationService _notifications;
    private readonly LoggerService _logger;
    private readonly CancellationTokenSource _cts;
    private Task _indexingTask;
    private volatile bool _isRunning;

    public event EventHandler<string>? ProgressChanged;
    public event EventHandler<string>? StatusChanged;

    public const int IndexingIntervalMinutes = 30;

    public BackgroundIndexer(
        IPackageProvider provider,
        NotificationService notifications,
        LoggerService logger)
    {
        _provider = provider;
        _notifications = notifications;
        _logger = logger;
        _cts = new CancellationTokenSource();
        _indexingTask = Task.CompletedTask;
    }

    public async Task<IReadOnlyList<InstalledPackage>> GetInstalledPackagesAsync()
    {
        StatusChanged?.Invoke(this, "Loading installed packages...");
        return await _provider.GetInstalledAsync();
    }

    public async Task<IReadOnlyList<UpdatePackage>> GetAvailableUpdatesAsync()
    {
        StatusChanged?.Invoke(this, "Loading available updates...");
        return await _provider.GetUpdatesAsync();
    }

    public async Task<WinGetXAvalonia.Models.PackageDetails?> GetPackageDetailsAsync(string packageId)
    {
        StatusChanged?.Invoke(this, $"Loading details for {packageId}...");
        return await _provider.GetDetailsAsync(packageId);
    }

    public async Task<IReadOnlyList<PackageModel>> SearchAsync(string query)
    {
        StatusChanged?.Invoke(this, $"Searching for {query}...");
        return await _provider.SearchAsync(query);
    }

    public async Task RefreshAsync()
    {
        if (_isRunning)
            return;

        try
        {
            _isRunning = true;
            StatusChanged?.Invoke(this, "Starting background refresh...");

            await Task.Run(async () =>
            {
                ProgressChanged?.Invoke(this, "1/3: Refreshing installed packages...");
                await _provider.GetInstalledAsync();

                ProgressChanged?.Invoke(this, "2/3: Refreshing available updates...");
                await _provider.GetUpdatesAsync();

                ProgressChanged?.Invoke(this, "3/3: Updating package catalog...");
                await _provider.SearchAsync("");

                StatusChanged?.Invoke(this, "Background refresh completed");

                _notifications.Notify(
                    NotificationType.Success,
                    "Background Refresh Complete",
                    $"All package data has been refreshed at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
            }, _cts.Token);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Background refresh failed");
            _notifications.Notify(
                NotificationType.Error,
                "Refresh Failed",
                "Unable to refresh package data. Please try again later.");
            throw;
        }
        finally
        {
            _isRunning = false;
        }
    }

    public void Start()
    {
        if (!_isRunning)
        {
            _indexingTask = IndexingLoopAsync(_cts.Token);
        }
    }

    public void Stop()
    {
        _cts.Cancel();
    }

    private async Task IndexingLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.UtcNow;
                var nextRun = now.AddMinutes(IndexingIntervalMinutes);

                while (now < nextRun && !cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, cancellationToken);
                    now = DateTime.UtcNow;
                }

                await RefreshAsync();
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Background indexing loop error");
                await Task.Delay(60000, cancellationToken);
            }
        }
    }
}
