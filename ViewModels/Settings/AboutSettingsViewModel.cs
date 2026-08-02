using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinGetXAvalonia.Infrastructure;
using WinGetXAvalonia.Services;
using WinGetXAvalonia.Services.Interfaces;

namespace WinGetXAvalonia.ViewModels.Settings;

public partial class AboutSettingsViewModel : SettingsCategoryViewModel
{
    private readonly WingetService _winget;

    public string AppName => AppInfo.ProductName;

    public string Description => AppInfo.Description;

    public string Version => AppInfo.Version;

    public string Commit
    {
        get
        {
            var informational =
                Assembly.GetExecutingAssembly()
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                    ?.InformationalVersion;

            var index = informational?.IndexOf('+') ?? -1;
            if (index <= 0)
                return "dev";

            var hash = informational![(index + 1)..];
            return hash.Length > 7 ? hash[..7] : hash;
        }
    }

    public string DotNet => RuntimeInformation.FrameworkDescription;

    public string Os => RuntimeInformation.OSDescription;

    public string Architecture => RuntimeInformation.OSArchitecture.ToString();

    public string Avalonia
        => typeof(Application).Assembly.GetName().Version?.ToString(3) ?? "?";

    [ObservableProperty]
    private string wingetVersion = "…";

    public AboutSettingsViewModel(
        ISettingsService settings,
        AutoSaveService autoSave,
        WingetService winget)
        : base("About", "ℹ", settings, autoSave)
    {
        _winget = winget;

        _ = LoadWingetVersionAsync();
    }

    [RelayCommand]
    private static void OpenGitHub()
        => Process.Start(new ProcessStartInfo
        {
            FileName = AppInfo.Repository,
            UseShellExecute = true
        });

    private async Task LoadWingetVersionAsync()
    {
        try
        {
            WingetVersion = (await _winget.Version()).Trim();
        }
        catch (Exception)
        {
            WingetVersion = "N/A";
        }
    }
}
