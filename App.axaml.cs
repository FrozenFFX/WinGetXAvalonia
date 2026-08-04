using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using WinGetXAvalonia.Core.Runtime;
using WinGetXAvalonia.ViewModels;
using WinGetXAvalonia.Views;

namespace WinGetXAvalonia;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            await InitializeAsync(desktop);
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static async Task InitializeAsync(
        IClassicDesktopStyleApplicationLifetime desktop)
    {
        var services = Program.Host!.Services;

        var runtime = services.GetRequiredService<RuntimeHost>();

        await runtime.StartAsync();

        var mainWindow = new MainWindow
        {
            DataContext = services.GetRequiredService<MainViewModel>(),
        };

        desktop.MainWindow = mainWindow;
        mainWindow.Show();
    }
}
