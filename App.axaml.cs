// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
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
                File.AppendAllText(
                    "crash.log",
                    $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Unhandled exception:{Environment.NewLine}{e.ExceptionObject}{Environment.NewLine}");
            }
            catch
            {
                // log dosyası yazılamazsa çökme raporu kaybolur, başka çare yok
            }
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

        var runtime = services.GetRequiredService<RuntimeHost>();

        await runtime.StartAsync();

        services.GetRequiredService<SoftwareIdentityEventListener>();

        var mainWindow = new MainWindow
        {
            DataContext = services.GetRequiredService<MainViewModel>(),
        };

        desktop.MainWindow = mainWindow;
        mainWindow.Show();
    }
}
