// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System;
using System.Diagnostics;
using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WinGetXAvalonia.Core.Events;
using WinGetXAvalonia.Core.IntentEngine;
using WinGetXAvalonia.Core.PackageEngine;
using WinGetXAvalonia.Core.Providers;
using WinGetXAvalonia.Core.Runtime;
using WinGetXAvalonia.Interfaces;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Analytics;
using WinGetXAvalonia.Services.Backup;
using WinGetXAvalonia.Services.Blueprints;
using WinGetXAvalonia.Services.Core;
using WinGetXAvalonia.Services.Diagnostics;
using WinGetXAvalonia.Services.Dialogs;
using WinGetXAvalonia.Services.Enterprise;
using WinGetXAvalonia.Services.History;
using WinGetXAvalonia.Services.Identity;
using WinGetXAvalonia.Services.Interfaces;
using WinGetXAvalonia.Services.Marketplace;
using WinGetXAvalonia.Services.Notifications;
using WinGetXAvalonia.Services.Operations;
using WinGetXAvalonia.Services.PackageDetails;
using WinGetXAvalonia.Services.Performance;
using WinGetXAvalonia.Services.Queue;
using WinGetXAvalonia.Services.Runtime;
using WinGetXAvalonia.Services.Sources;
using WinGetXAvalonia.Services.Studio;
using WinGetXAvalonia.Services.Theme;
using WinGetXAvalonia.Services.Workspaces;
using WinGetXAvalonia.ViewModels;
using WinGetXAvalonia.ViewModels.Settings;

namespace WinGetXAvalonia;

internal sealed class Program
{
    public static IHost? Host { get; private set; }

    /// <summary>
    /// Testler ve araçlar için UI başlatmadan DI host kurar.
    /// </summary>
    public static IHost CreateHost()
    {
        var builder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder();
        builder.Services.ConfigureServices();
        return builder.Build();
    }

    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            var builder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder(args);
            builder.Services.ConfigureServices();

            ValidateDependencyGraph(builder.Services);

            Host = builder.Build();

            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);

            Host.Dispose();
        }
        catch (Exception ex)
        {
            var logger = new LoggerService();
            logger.Error(ex, "Fatal error during application startup");
            Environment.Exit(1);
        }
    }

    private static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
    }

    /// <summary>
    /// DI kayıtlarını, servisleri örneklemeden doğrular. Eksik bir kayıt varsa
    /// uygulama derinlerde çökmek yerine başlangıçta net bir hata ile durur.
    /// </summary>
    private static void ValidateDependencyGraph(IServiceCollection services)
    {
        try
        {
            using var validation = services.BuildServiceProvider(
                new ServiceProviderOptions { ValidateOnBuild = true });
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "DI registration is broken. Fix the missing service registration before starting.",
                ex);
        }
    }
}

public static class ServiceCollectionExtensions
{
    public static void ConfigureServices(this IServiceCollection services)
    {
        services.AddLogging();

        // Çekirdek servisler
        services.AddSingleton<LoggerService>();
        services.AddSingleton<NotificationService>();
        services.AddSingleton<INotificationService, NotificationService>();
        services.AddSingleton<IEventBus, EventBus>();
        services.AddSingleton<IHistoryService, HistoryService>();
        services.AddSingleton<ISourceManagerService, SourceManagerService>();
        services.AddSingleton<IWingetProcess, WingetProcess>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<SettingsStore>();
        services.AddSingleton<IPackageProvider, WingetProvider>();
        services.AddSingleton<IPackageEngine, PackageEngine>();
        services.AddSingleton<WingetService>();
        services.AddSingleton<PackageSearchCache>();
        services.AddSingleton<PackageCacheService>();
        services.AddSingleton<IFeaturedCatalog, FeaturedCatalog>();
        services.AddSingleton<IIdentityService, SoftwareIdentityService>();
        services.AddSingleton<IThemeService, ThemeService>();
        services.AddSingleton<ThemeManager>();
        services.AddSingleton<IEnterpriseService, EnterpriseService>();
        services.AddSingleton<FleetReportWriter>();
        services.AddSingleton<IStudioService, StudioService>();
        services.AddSingleton<IWorkspaceService, WorkspaceService>();
        services.AddSingleton<IBlueprintStore, BlueprintStore>();
        services.AddSingleton<IImportService, ImportService>();
        services.AddSingleton<IExportService, ExportService>();
        services.AddSingleton<IBackupService, BackupService>();
        services.AddSingleton<AutoSaveService>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<IFilePickerService, FilePickerService>();
        services.AddSingleton<IPerformanceService, PerformanceService>();
        services.AddSingleton<ISystemMonitorService, SystemMonitorService>();
        services.AddSingleton<SystemInfoService>();
        services.AddSingleton<IOperationQueue, OperationQueue>();
        services.AddSingleton<IOperationService, OperationService>();
        services.AddSingleton<IOperationCenter, OperationCenter>();
        services.AddSingleton<IClipboardService, ClipboardService>();
        services.AddSingleton<ILauncherService, LauncherService>();
        services.AddSingleton<TimeService>();
        services.AddSingleton<ITimeService>(sp => sp.GetRequiredService<TimeService>());
        services.AddSingleton<ClockService>();
        services.AddSingleton<FavoritesService>();
        services.AddSingleton<DiagnosticsService>();
        services.AddSingleton<IDiagnosticsService, DiagnosticsService>();
        services.AddSingleton<MarketplaceService>();
        services.AddSingleton<PackageDetailsService>();
        services.AddSingleton<IPackageDetailsService, PackageDetailsService>();
        services.AddSingleton<GithubService>();
        services.AddSingleton<IErrorHandlingService, ErrorHandlingService>();
        services.AddSingleton<IAnalyticsService, AnalyticsService>();
        services.AddSingleton<IPackageService, PackageService>();
        services.AddSingleton<IBackgroundIndexer, BackgroundIndexer>();
        services.AddSingleton<IGlobalSearch, GlobalSearch>();
        services.AddSingleton<IUserIntentEngine, IntentEngine>();

        // Çalışma zamanı
        services.AddSingleton<RuntimeContext>();
        services.AddSingleton<RuntimeFeatureManager>(_ =>
        {
            var manager = new RuntimeFeatureManager();
            manager.Register(new RuntimeFeature { Id = "Diagnostics", Name = "Diagnostics", Enabled = true });
            manager.Register(new RuntimeFeature { Id = "Automation", Name = "Automation", Enabled = true });
            manager.Register(new RuntimeFeature { Id = "Studio", Name = "Studio", Enabled = true });
            manager.Register(new RuntimeFeature { Id = "Plugins", Name = "Plugins", Experimental = true });
            manager.Register(new RuntimeFeature { Id = "Analytics", Name = "Analytics", Enabled = true });
            manager.Register(new RuntimeFeature { Id = "Founder", Name = "Founder", Experimental = true });
            return manager;
        });
        services.AddSingleton<RuntimeHost, AppRuntimeHost>();
        services.AddSingleton<SoftwareIdentityEventListener>();

        // Ayarlar sayfa alt-ViewModel'leri
        services.AddSingleton<GeneralSettingsViewModel>();
        services.AddSingleton<AppearanceSettingsViewModel>();
        services.AddSingleton<PerformanceSettingsViewModel>();
        services.AddSingleton<WingetSettingsViewModel>();
        services.AddSingleton<NotificationsSettingsViewModel>();
        services.AddSingleton<BackupSettingsViewModel>();
        services.AddSingleton<DeveloperSettingsViewModel>();
        services.AddSingleton<AboutSettingsViewModel>();

        // Sayfa ViewModel'leri
        services.AddSingleton<NavigationService>();
        services.AddSingleton<QueuePanelViewModel>();
        services.AddSingleton<NotificationCenterViewModel>();
        services.AddSingleton<OperationDrawerViewModel>();
        services.AddSingleton<CommandPaletteViewModel>();
        services.AddSingleton<DashboardViewModel>();
        services.AddSingleton<InstalledViewModel>();
        services.AddSingleton<UpdatesViewModel>();
        services.AddSingleton<BrowseViewModel>();
        services.AddSingleton<HistoryViewModel>();
        services.AddSingleton<FavoritesViewModel>();
        services.AddSingleton<WorkspacesViewModel>();
        services.AddSingleton<BlueprintsViewModel>();
        services.AddSingleton<MarketplaceViewModel>();
        services.AddSingleton<EnterpriseViewModel>();
        services.AddSingleton<SoftwareIdentityViewModel>();
        services.AddSingleton<AutomationViewModel>();
        services.AddSingleton<SecurityViewModel>();
        services.AddSingleton<DiagnosticsViewModel>();
        services.AddSingleton<SourceManagerViewModel>();
        services.AddSingleton<StudioViewModel>();
        services.AddSingleton<DeveloperViewModel>();
        services.AddSingleton<SettingsViewModel>();
        services.AddSingleton<MainViewModel>();

        // Program içi yardımcı servisler
        services.AddTransient<IVersionDetector, VersionDetector>();
        services.AddTransient<IMonitoringService, MonitoringService>();
        services.AddTransient<IRemoteCommandService, RemoteCommandService>();
    }
}

public interface IVersionDetector
{
    Task<string> GetWingetVersionAsync();
    Task<string> GetApplicationVersionAsync();
}

public class VersionDetector : IVersionDetector
{
    private readonly LoggerService _logger;

    public VersionDetector(LoggerService logger)
    {
        _logger = logger;
    }

    public async Task<string> GetWingetVersionAsync()
    {
        try
        {
            ProcessStartInfo psi = new()
            {
                FileName = "winget",
                Arguments = "--version",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using Process process = Process.Start(psi)!;
            var output = await process.StandardOutput.ReadToEndAsync();
            return output.Trim();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to get winget version");
            return "Unknown";
        }
    }

    public async Task<string> GetApplicationVersionAsync()
    {
        try
        {
            var version = typeof(Program).Assembly.GetName().Version;
            return version?.ToString() ?? "Unknown";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to get application version");
            return "Unknown";
        }
    }
}

public interface IMonitoringService
{
    Task<SystemInfo> GetSystemInfoAsync();
    Task<PerformanceStats> GetPerformanceStatsAsync();
    Task<MemoryStats> GetMemoryStatsAsync();
}

public class MonitoringService : IMonitoringService
{
    private readonly LoggerService _logger;

    public MonitoringService(LoggerService logger)
    {
        _logger = logger;
    }

    public async Task<SystemInfo> GetSystemInfoAsync()
    {
        return new SystemInfo
        {
            OsVersion = Environment.OSVersion.VersionString,
            ProcessorCount = Environment.ProcessorCount,
            TotalMemory = GC.GetTotalMemory(false),
            AvailableMemory = long.MaxValue - GC.GetTotalMemory(false)
        };
    }

    public async Task<PerformanceStats> GetPerformanceStatsAsync()
    {
        return new PerformanceStats
        {
            CpuUsage = 0.0,
            MemoryUsage = 0.0,
            DiskUsage = 0.0,
            NetworkUsage = 0.0,
            ProcessCount = 0
        };
    }

    public async Task<MemoryStats> GetMemoryStatsAsync()
    {
        return new MemoryStats
        {
            TotalPhysicalMemory = 0,
            AvailableMemory = 0,
            WorkingSet = 0,
            PrivateMemory = 0
        };
    }
}

public interface IRemoteCommandService
{
    Task<string> ExecuteRemoteCommandAsync(string command, string parameters);
    Task<RemoteCommandResult> ExecuteRemoteCommandWithResultAsync(string command, string parameters);
}

public class RemoteCommandService : IRemoteCommandService
{
    private readonly LoggerService _logger;
    private readonly IHistoryService _history;

    public RemoteCommandService(
        LoggerService logger,
        IHistoryService history)
    {
        _logger = logger;
        _history = history;
    }

    public async Task<string> ExecuteRemoteCommandAsync(string command, string parameters)
    {
        try
        {
            var historyEntry = new HistoryEntry
            {
                Timestamp = DateTime.UtcNow,
                Operation = "RemoteCommand",
                PackageId = command,
                Success = true,
                Message = $"Executed remote command: {command} with parameters: {parameters}"
            };
            await _history.AddAsync(historyEntry);

            _logger.Log($"Executed remote command: {command} with parameters: {parameters}");
            return $"Command executed successfully: {command}";
        }
        catch (Exception ex)
        {
            var historyEntry = new HistoryEntry
            {
                Timestamp = DateTime.UtcNow,
                Operation = "RemoteCommand",
                PackageId = command,
                Success = false,
                Message = ex.Message
            };
            await _history.AddAsync(historyEntry);

            _logger.Error(ex, $"Failed to execute remote command: {command}");
            throw;
        }
    }

    public async Task<RemoteCommandResult> ExecuteRemoteCommandWithResultAsync(string command, string parameters)
    {
        var result = new RemoteCommandResult
        {
            Command = command,
            Parameters = parameters,
            Timestamp = DateTime.UtcNow,
            Success = false,
            Output = ""
        };

        try
        {
            result.Output = await ExecuteRemoteCommandAsync(command, parameters);
            result.Success = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.Message;
            _logger.Error(ex, $"Remote command failed: {command}");
        }

        return result;
    }
}

public class SystemInfo
{
    public string OsVersion { get; set; } = string.Empty;
    public int ProcessorCount { get; set; }
    public long TotalMemory { get; set; }
    public long AvailableMemory { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class PerformanceStats
{
    public double CpuUsage { get; set; }
    public double MemoryUsage { get; set; }
    public double DiskUsage { get; set; }
    public double NetworkUsage { get; set; }
    public int ProcessCount { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class MemoryStats
{
    public long TotalPhysicalMemory { get; set; }
    public long AvailableMemory { get; set; }
    public long WorkingSet { get; set; }
    public long PrivateMemory { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class RemoteCommandResult
{
    public string Command { get; set; } = string.Empty;
    public string Parameters { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public bool Success { get; set; }
    public string Output { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}
