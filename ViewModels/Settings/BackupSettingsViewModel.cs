using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinGetXAvalonia.Services;
using WinGetXAvalonia.Services.Interfaces;

namespace WinGetXAvalonia.ViewModels.Settings;

public partial class BackupSettingsViewModel : SettingsCategoryViewModel
{
    [ObservableProperty]
    private bool autoBackup;

    [ObservableProperty]
    private string? statusMessage;

    public BackupSettingsViewModel(
        ISettingsService settings,
        AutoSaveService autoSave)
        : base("Backup", "💾", settings, autoSave)
    {
        BeginInit();

        AutoBackup = Settings.Settings.AutoBackup;

        EndInit();
    }

    partial void OnAutoBackupChanged(bool value)
        => Queue(s => s.AutoBackup = value);

    [RelayCommand]
    private async Task ExportSettings()
    {
        var file = await PickSaveFileAsync();
        if (file is null)
            return;

        await Settings.ExportAsync(file.Path.LocalPath);
        StatusMessage = "Settings exported.";
    }

    [RelayCommand]
    private async Task ImportSettings()
    {
        var file = await PickOpenFileAsync();
        if (file is null)
            return;

        var ok = await Settings.ImportAsync(file.Path.LocalPath);
        StatusMessage = ok ? "Settings imported." : "Import failed: invalid file.";
    }

    [RelayCommand]
    private async Task ResetSettings()
    {
        await Settings.ResetAsync();
        StatusMessage = "Settings reset to defaults.";
    }

    private static async Task<IStorageFile?> PickSaveFileAsync()
    {
        var window = GetWindow();
        if (window is null)
            return null;

        return await window.StorageProvider.SaveFilePickerAsync(
            new FilePickerSaveOptions
            {
                Title = "Export Settings",
                SuggestedFileName = "WinGetX-settings",
                DefaultExtension = "json",
                FileTypeChoices = [new FilePickerFileType("JSON") { Patterns = ["*.json"] }]
            });
    }

    private static async Task<IStorageFile?> PickOpenFileAsync()
    {
        var window = GetWindow();
        if (window is null)
            return null;

        var files = await window.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = "Import Settings",
                AllowMultiple = false,
                FileTypeFilter = [new FilePickerFileType("JSON") { Patterns = ["*.json"] }]
            });

        return files.FirstOrDefault();
    }

    private static Window? GetWindow()
        => Application.Current?.ApplicationLifetime
               is IClassicDesktopStyleApplicationLifetime
           {
               MainWindow: Window window
           }
               ? window
               : null;
}
