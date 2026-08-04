using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WinGetXAvalonia.Core.Providers;
using WinGetXAvalonia.Core.Events;
using WinGetXAvalonia.Interfaces;
using WinGetXAvalonia.Services;
using WinGetXAvalonia.Services.Core;
using WinGetXAvalonia.Services.Performance;
using WinGetXAvalonia.Services.Backup;
using WinGetXAvalonia.Services.Dialogs;
using WinGetXAvalonia.Services.History;
using WinGetXAvalonia.Services.Interfaces;
using WinGetXAvalonia.Services.Notifications;
using WinGetXAvalonia.Services.Operations;
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
                services.AddSingleton<LoggerService>();
                services.AddSingleton<GithubService>();
                services.AddSingleton<ClockService>();
                services.AddSingleton<IClipboardService, ClipboardService>();
                services.AddSingleton<ILauncherService, LauncherService>();
                services.AddSingleton<IHistoryService, HistoryService>();
                services.AddSingleton<IWorkspaceService, WorkspaceService>();
                services.AddSingleton<IDialogService, DialogService>();
                services.AddSingleton<IEventBus, EventBus>();
                services.AddSingleton<IOperationService, OperationService>();
                services.AddSingleton<ISystemMonitorService, SystemMonitorService>();
                services.AddSingleton<ITimeService, TimeService>();
                services.AddSingleton<IOperationQueue, OperationQueue>();

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
