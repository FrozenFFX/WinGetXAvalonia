// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinGetXAvalonia.Core.State;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Backup;
using WinGetXAvalonia.Services.Dialogs;

namespace WinGetXAvalonia.ViewModels;

public partial class BlueprintsViewModel : AsyncViewModel
{
    private readonly IExportService _export;
    private readonly IDialogService _dialogs;
    private readonly IFilePickerService _filePicker;

    public ObservableCollection<BlueprintModel> Blueprints { get; } = new();

    [ObservableProperty]
    private BlueprintModel? selectedBlueprint;

    [ObservableProperty]
    private string name = "";

    [ObservableProperty]
    private string description = "";

    [RelayCommand]
    private void New()
    {
        SelectedBlueprint = null;

        Name = "";
        Description = "";
    }

    [RelayCommand]
    private void Save()
    {
    }

    [RelayCommand]
    private void Delete()
    {
    }

    public BlueprintsViewModel(
        IExportService export,
        IDialogService dialogs,
        IFilePickerService filePicker)
    {
        _export = export;
        _dialogs = dialogs;
        _filePicker = filePicker;
    }

    [RelayCommand]
    private async Task Export()
    {
        var file = await _filePicker.PickSaveJsonAsync(
            "Export Blueprints",
            $"blueprints-{DateTime.Now:yyyyMMdd-HHmmss}.json");

        if (file is null)
            return;

        try
        {
            await _export.ExportJsonAsync(
                Blueprints.ToList(),
                file.Path.LocalPath);
        }
        catch (Exception ex)
        {
            await _dialogs.ShowErrorAsync("Export Failed", ex.Message);
        }
    }

    [RelayCommand]
    public Task Refresh()
    {
        return Task.CompletedTask;
    }

    public Task InitializeAsync()
    {
        return Refresh();
    }
}
