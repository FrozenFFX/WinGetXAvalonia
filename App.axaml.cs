// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WinGetXAvalonia.Core.Runtime;
using WinGetXAvalonia.Services.Identity;
using WinGetXAvalonia.ViewModels;
using WinGetXAvalonia.Views;

namespace WinGetXAvalonia;

public partial class App : Application
{
    public override void Initialize()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            try
            {
                var msg = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Unhandled exception:{Environment.NewLine}{e.ExceptionObject}{Environment.NewLine}";
                File.AppendAllText("crash.log", msg);
                try { Console.WriteLine(msg); } catch { }
            }
            catch
            {
      
            }
        };

        // Tek sayfa/template hatası uygulamayı öldürmesin. Log çıkar ve devam et.
        // Gerçek çökme kök nedenleri yine de düzeltilir; bu yalnızca beklenmedik
        // durumlar için emniyet ağıdır (ARTICLE VII).
        Dispatcher.UIThread.UnhandledException += (_, e) =>
        {
            try
            {
                var msg = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Dispatcher unhandled exception:{Environment.NewLine}{e.Exception}{Environment.NewLine}";
                File.AppendAllText("crash.log", msg);
                try { Console.WriteLine(msg); } catch { }
            }
            catch
            {
            }

            e.Handled = true;
        };

        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Exit += (_, _) =>
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            };

            try
            {
                await InitializeAsync(desktop);
            }
            catch (Exception ex)
            {
                try
                {
                    File.AppendAllText(
                        "crash.log",
                        $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Startup failed:{Environment.NewLine}{ex}{Environment.NewLine}");
                    
                    try { Console.WriteLine($"[STARTUP ERROR] {ex.Message}"); } catch { }
                }
                catch
                {
                }

                throw;
            }
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static async Task InitializeAsync(
        IClassicDesktopStyleApplicationLifetime desktop)
    {
        var services = Program.Host!.Services;
        var logger = services.GetRequiredService<ILogger<App>>();

        try
        {
            logger.LogInformation("[STARTUP] Initializing application...");
            
            var runtime = services.GetRequiredService<RuntimeHost>();
            logger.LogInformation("[STARTUP] Runtime start started...");
            
            await runtime.StartAsync();
            logger.LogInformation($"[STARTUP] Runtime started. State: {runtime.Context.State}");

            logger.LogInformation("[STARTUP] Registering SoftwareIdentityEventListener...");
            services.GetRequiredService<SoftwareIdentityEventListener>();
            logger.LogInformation("[STARTUP] SoftwareIdentityEventListener registered");

            logger.LogInformation("[STARTUP] Creating MainWindow...");
            var mainWindow = new MainWindow
            {
                DataContext = services.GetRequiredService<MainViewModel>(),
            };
            desktop.MainWindow = mainWindow;
            mainWindow.Show();
            logger.LogInformation("[STARTUP] MainWindow shown");
        }
        catch (Exception ex)
        {
            try
            {
                var msg = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Startup failed:{Environment.NewLine}{ex}{Environment.NewLine}";
                File.AppendAllText("crash.log", msg);
                try { Console.WriteLine(msg); } catch { }
            }
            catch { }
            throw;
        }
    }
}