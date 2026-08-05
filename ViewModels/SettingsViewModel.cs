// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Backup;
using WinGetXAvalonia.Services.Dialogs;
using WinGetXAvalonia.ViewModels.Settings;

namespace WinGetXAvalonia.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly IBackupService _backup;
    private readonly IDialogService _dialogs;
    private readonly IFilePickerService _filePicker;

    public IReadOnlyList<SettingsCategoryViewModel> Categories { get; }

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private SettingsCategoryViewModel? selectedCategory;

    public SettingsViewModel(
        GeneralSettingsViewModel general,
        AppearanceSettingsViewModel appearance,
        PerformanceSettingsViewModel performance,
        WingetSettingsViewModel winget,
        NotificationsSettingsViewModel notifications,
        BackupSettingsViewModel backup,
        DeveloperSettingsViewModel developer,
        AboutSettingsViewModel about,
        IBackupService backupService,
        IDialogService dialogs,
        IFilePickerService filePicker)
    {
        _backup = backupService;
        _dialogs = dialogs;
        _filePicker = filePicker;
        Categories = new SettingsCategoryViewModel[]
        {
            general,
            appearance,
            performance,
            winget,
            notifications,
            backup,
            developer,
            about
        };

        SelectedCategory = general;
    }

    [RelayCommand]
    private async Task Backup()
    {
        var file = await _filePicker.PickSaveJsonAsync(
            "Create Backup",
            $"backup-{DateTime.Now:yyyyMMdd-HHmmss}.wgx");

        if (file is null)
            return;

        try
        {
            await _backup.CreateAsync(
                file.Path.LocalPath,
                new BackupModel
                {
                    Created = DateTime.Now
                });
        }
        catch (Exception ex)
        {
            await _dialogs.ShowErrorAsync("Backup Failed", ex.Message);
        }
    }

    [RelayCommand]
    private async Task Restore()
    {
        var file = await _filePicker.PickOpenJsonAsync(
            "Restore Backup");

        if (file is null)
            return;

        try
        {
            await _backup.LoadAsync(file.Path.LocalPath);
        }
        catch (Exception ex)
        {
            await _dialogs.ShowErrorAsync("Restore Failed", ex.Message);
        }
    }
}
