// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Runtime;
using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WinGetXAvalonia.Core.PackageEngine;
using WinGetXAvalonia.Core.Providers;
using WinGetXAvalonia.Core.Events;
using WinGetXAvalonia.Core.Runtime;
using WinGetXAvalonia.Services.Runtime;
using WinGetXAvalonia.Interfaces;
using WinGetXAvalonia.Services;
using WinGetXAvalonia.Services.Core;
using WinGetXAvalonia.Services.Performance;
using WinGetXAvalonia.Services.Backup;
using WinGetXAvalonia.Services.Dialogs;
using WinGetXAvalonia.Services.Diagnostics;
using WinGetXAvalonia.Services.History;
using WinGetXAvalonia.Services.Interfaces;
using WinGetXAvalonia.Services.Notifications;
using WinGetXAvalonia.Services.Operations;
using WinGetXAvalonia.Services.PackageDetails;
using WinGetXAvalonia.Services.Queue;
using WinGetXAvalonia.Services.Studio;
using WinGetXAvalonia.Services.Theme;
using WinGetXAvalonia.Services.Workspaces;
using WinGetXAvalonia.ViewModels;
using WinGetXAvalonia.ViewModels.Settings;

namespace WinGetXAvalonia;

class Program
{
    public static IHost? Host;

    [STAThread]
    public static void Main(string[] args)
    {
        GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

        Host = Microsoft.Extensions.Hosting.Host
            .CreateDefaultBuilder(args)
            .ConfigureServices((_, services) =>
            {
                services.AddSingleton<MainViewModel>();

                services.AddSingleton<DashboardViewModel>();
                services.AddSingleton<InstalledViewModel>();
                services.AddSingleton<UpdatesViewModel>();
                services.AddSingleton<BrowseViewModel>();
                services.AddSingleton<HistoryViewModel>();
                services.AddSingleton<FavoritesViewModel>();
                services.AddSingleton<WorkspacesViewModel>();
                services.AddSingleton<BlueprintsViewModel>();
                services.AddSingleton<AutomationViewModel>();
                services.AddSingleton<SecurityViewModel>();
                services.AddSingleton<DiagnosticsViewModel>();
                services.AddSingleton<StudioViewModel>();
                services.AddSingleton<DeveloperViewModel>();
                services.AddSingleton<SettingsViewModel>();

                services.AddSingleton<QueuePanelViewModel>();

                services.AddSingleton<NotificationService>();
                services.AddSingleton<NotificationCenterViewModel>();

                services.AddSingleton<WingetService>();
                services.AddSingleton<IPackageProvider, WingetProvider>();
                services.AddSingleton<PackageSearchCache>();
                services.AddSingleton<IFeaturedCatalog, FeaturedCatalog>();
                services.AddSingleton<NavigationService>();
                services.AddSingleton<SettingsStore>();
                services.AddSingleton<ISettingsService, SettingsService>();
                services.AddSingleton<IThemeService, ThemeService>();
                services.AddSingleton<ThemeManager>();
                services.AddSingleton<AutoSaveService>();
                services.AddSingleton<FavoritesService>();
                services.AddSingleton<SystemInfoService>();
                services.AddSingleton<IDiagnosticsService, DiagnosticsService>();
                services.AddSingleton<LoggerService>();
                services.AddSingleton<GithubService>();
                services.AddSingleton<ClockService>();
                services.AddSingleton<IClipboardService, ClipboardService>();
                services.AddSingleton<ILauncherService, LauncherService>();
                services.AddSingleton<IHistoryService, HistoryService>();
                services.AddSingleton<WorkspaceSerializer>();
                services.AddSingleton<WorkspaceImporter>();
                services.AddSingleton<WorkspaceExporter>();
                services.AddSingleton<WorkspaceService>();
                services.AddSingleton<IWorkspaceService, WorkspaceService>();
                services.AddSingleton<IStudioService, StudioService>();
                services.AddSingleton<IDialogService, DialogService>();
                services.AddSingleton<IFilePickerService, FilePickerService>();
                services.AddSingleton<IEventBus, EventBus>();
                services.AddSingleton<IOperationService, OperationService>();
                services.AddSingleton<IPackageDetailsService, PackageDetailsService>();
                services.AddSingleton<IPackageEngine, PackageEngine>();
                services.AddSingleton<PackagePipeline>();
                services.AddSingleton<ISystemMonitorService, SystemMonitorService>();
                services.AddSingleton<ITimeService, TimeService>();
                services.AddSingleton<IOperationQueue, OperationQueue>();
                services.AddSingleton<IOperationCenter, OperationCenter>();
                services.AddSingleton<OperationDrawerViewModel>();

                services.AddSingleton<RuntimeContext>();
                services.AddSingleton<RuntimeFeatureManager>(_ =>
                {
                    var manager = new RuntimeFeatureManager();
                    manager
                        .Register(new RuntimeFeature { Id = "Diagnostics", Name = "Diagnostics", Enabled = true })
                        .Register(new RuntimeFeature { Id = "Automation", Name = "Automation", Enabled = true })
                        .Register(new RuntimeFeature { Id = "Studio", Name = "Studio", Enabled = true })
                        .Register(new RuntimeFeature { Id = "Plugins", Name = "Plugins", Experimental = true })
                        .Register(new RuntimeFeature { Id = "Analytics", Name = "Analytics", Enabled = true })
                        .Register(new RuntimeFeature { Id = "Founder", Name = "Founder", Experimental = true });
                    return manager;
                });
                services.AddSingleton<RuntimeHost, AppRuntimeHost>();

                services.AddSingleton<GeneralSettingsViewModel>();
                services.AddSingleton<AppearanceSettingsViewModel>();
                services.AddSingleton<PerformanceSettingsViewModel>();
                services.AddSingleton<WingetSettingsViewModel>();
                services.AddSingleton<NotificationsSettingsViewModel>();
                services.AddSingleton<BackupSettingsViewModel>();
                services.AddSingleton<DeveloperSettingsViewModel>();
                services.AddSingleton<AboutSettingsViewModel>();
            })
            .Build();

        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
#if DEBUG
            .WithDeveloperTools()
#endif
            .LogToTrace();
}
