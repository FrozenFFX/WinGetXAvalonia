using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WinGetXAvalonia.Services;
using WinGetXAvalonia.Services.Interfaces;
using WinGetXAvalonia.Services.Theme;
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
                services.AddSingleton<FavoritesViewModel>();
                services.AddSingleton<SettingsViewModel>();

                services.AddSingleton<WingetService>();
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
