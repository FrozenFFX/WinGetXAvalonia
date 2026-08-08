// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinGetXAvalonia.Core.State;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Backup;
using WinGetXAvalonia.Services.Blueprints;
using WinGetXAvalonia.Services.Dialogs;

namespace WinGetXAvalonia.ViewModels;

public partial class BlueprintsViewModel : AsyncViewModel
{
    private readonly IExportService _export;
    private readonly IDialogService _dialogs;
    private readonly IFilePickerService _filePicker;
    private readonly IBlueprintStore? _store;

    public ObservableCollection<BlueprintModel> Blueprints { get; } = new();

    public ObservableCollection<BlueprintModel> FilteredBlueprints { get; } = new();

    [ObservableProperty]
    private BlueprintModel? selectedBlueprint;

    [ObservableProperty]
    private string name = "";

    [ObservableProperty]
    private string description = "";

    [ObservableProperty]
    private string steps = "";

    [ObservableProperty]
    private string searchText = "";

    public bool HasNoItems => FilteredBlueprints.Count == 0;

    public bool IsEditing => SelectedBlueprint is not null;

    public string EditorTitle =>
        SelectedBlueprint?.Name is { Length: > 0 } title ? title : "New Blueprint";

    public string EditorDescription => SelectedBlueprint is null
        ? "Create a reusable installation template."
        : "Editing a saved blueprint — changes apply when you save.";

    public BlueprintsViewModel(
        IExportService export,
        IDialogService dialogs,
        IFilePickerService filePicker,
        IBlueprintStore? store = null)
    {
        _export = export;
        _dialogs = dialogs;
        _filePicker = filePicker;
        _store = store;

        Blueprints.CollectionChanged += (_, _) => ApplyFilter();
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    partial void OnSelectedBlueprintChanged(BlueprintModel? value)
    {
        if (value is null)
        {
            Name = "";
            Description = "";
            Steps = "";
        }
        else
        {
            Name = value.Name;
            Description = value.Description;
            Steps = value.Steps;
        }

        OnPropertyChanged(nameof(IsEditing));
        OnPropertyChanged(nameof(EditorTitle));
        OnPropertyChanged(nameof(EditorDescription));
    }

    [RelayCommand]
    private void New()
    {
        SelectedBlueprint = null;
        Name = "";
        Description = "";
        Steps = "";
    }

    [RelayCommand]
    private async Task Save()
    {
        if (string.IsNullOrWhiteSpace(Name))
            return;

        if (SelectedBlueprint is { } existing)
        {
            existing.Name = Name.Trim();
            existing.Description = Description.Trim();
            existing.Steps = Steps.Trim();
        }
        else
        {
            var created = new BlueprintModel
            {
                Name = Name.Trim(),
                Description = Description.Trim(),
                Steps = Steps.Trim()
            };

            Blueprints.Insert(0, created);
            SelectedBlueprint = created;
        }

        if (_store is { } store)
        {
            try
            {
                await store.SaveAsync(SelectedBlueprint!);
            }
            catch (Exception ex)
            {
                await _dialogs.ShowErrorAsync("Save Failed", ex.Message);
            }
        }

        OnPropertyChanged(nameof(EditorTitle));
    }

    [RelayCommand]
    private async Task Delete()
    {
        if (SelectedBlueprint is not { } target)
            return;

        bool confirmed = await _dialogs.ConfirmAsync(
            "Delete Blueprint",
            $"Delete '{target.Name}'? This cannot be undone.");

        if (!confirmed)
            return;

        if (_store is { } store)
        {
            try
            {
                await store.DeleteAsync(target.Id);
            }
            catch (Exception ex)
            {
                await _dialogs.ShowErrorAsync("Delete Failed", ex.Message);
            }
        }

        Blueprints.Remove(target);
        SelectedBlueprint = null;
    }

    [RelayCommand]
    private async Task Import()
    {
        if (_store is { } store)
        {
            await ImportViaStore(store);
            return;
        }

        var file = await _filePicker.PickOpenJsonAsync("Import Blueprints");

        if (file is null)
            return;

        try
        {
            string text = await File.ReadAllTextAsync(file.Path.LocalPath);

            var items = JsonSerializer.Deserialize<List<BlueprintModel>>(text);

            if (items is null)
                return;

            foreach (var item in items)
            {
                item.Id = Guid.NewGuid();
                Blueprints.Add(item);
            }
        }
        catch (Exception ex)
        {
            await _dialogs.ShowErrorAsync("Import Failed", ex.Message);
        }
    }

    private async Task ImportViaStore(IBlueprintStore store)
    {
        var file = await _filePicker.PickOpenJsonAsync("Import Blueprints");

        if (file is null)
            return;

        try
        {
            string text = await File.ReadAllTextAsync(file.Path.LocalPath);

            var imported = await store.ImportJsonAsync(text);

            foreach (var item in imported)
                Blueprints.Add(item);
        }
        catch (Exception ex)
        {
            await _dialogs.ShowErrorAsync("Import Failed", ex.Message);
        }
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
    public async Task Refresh()
    {
        if (_store is not { } store)
            return;

        try
        {
            var items = await store.GetAllAsync();
            ApplyLoaded(items);
        }
        catch (Exception ex)
        {
            await _dialogs.ShowErrorAsync("Load Failed", ex.Message);
        }
    }

    private void ApplyLoaded(IEnumerable<BlueprintModel> items)
    {
        Blueprints.Clear();

        foreach (var item in items)
            Blueprints.Add(item);
    }

    public override Task InitializeAsync()
    {
        return Refresh();
    }

    private void ApplyFilter()
    {
        IEnumerable<BlueprintModel> filtered = Blueprints;

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            filtered = filtered.Where(b =>
                b.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                || b.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
        }

        var matches = filtered.ToList();

        if (SelectedBlueprint is not null && !matches.Contains(SelectedBlueprint))
            SelectedBlueprint = null;

        FilteredBlueprints.Clear();

        foreach (var item in matches)
            FilteredBlueprints.Add(item);

        OnPropertyChanged(nameof(HasNoItems));
    }
}

