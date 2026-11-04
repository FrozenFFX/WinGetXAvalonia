// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System;
using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WinGetXAvalonia.Core.Events;
using WinGetXAvalonia.Core.IntentEngine;
using WinGetXAvalonia.Core.PackageEngine;
using WinGetXAvalonia.Core.Providers;
using WinGetXAvalonia.Core.Runtime;
using WinGetXAvalonia.Core.State;
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
using WinGetXAvalonia.Services.Localization;
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
using WinGetXAvalonia.Services.Updates;
using WinGetXAvalonia.Services.Workspaces;
using WinGetXAvalonia.ViewModels;
using WinGetXAvalonia.ViewModels.Settings;

namespace WinGetXAvalonia;

internal sealed class Program
{
    public static IHost? Host { get; private set; }

    /// <summary>
    /// Testler ve araÃ§lar iÃ§in UI baÅŸlatmadan DI host kurar.
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
    /// DI kayÄ±tlarÄ±nÄ±, servisleri Ã¶rneklemeden doÄŸrular. Eksik bir kayÄ±t varsa
    /// uygulama derinlerde Ã§Ã¶kmek yerine baÅŸlangÄ±Ã§ta net bir hata ile durur.
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

        // Ã‡ekirdek servisler
        services.AddSingleton<LoggerService>();
        services.AddSingleton<NotificationService>();
        services.AddSingleton<INotificationService, NotificationService>();
        services.AddSingleton<IEventBus, EventBus>();
        services.AddSingleton<IHistoryService, HistoryService>();
        services.AddSingleton<ISourceManagerService, SourceManagerService>();
        services.AddSingleton<IWingetProcess, WingetProcess>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<LocalizationManager>();
        services.AddSingleton<SettingsStore>();
        services.AddSingleton<IPackageProvider, WingetProvider>();
        services.AddSingleton<IPackageEngine, PackageEngine>();
        services.AddSingleton<WingetService>();
        services.AddSingleton<IUpdateService, UpdateService>();
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
        services.AddSingleton<SystemInformation>(SystemInformation.Empty);
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

        // Ã‡alÄ±ÅŸma zamanÄ±
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
        services.AddSingleton<GameModeViewModel>();
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
    }
}
