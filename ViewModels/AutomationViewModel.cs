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
using WinGetXAvalonia.Services.Dialogs;

namespace WinGetXAvalonia.ViewModels;

public partial class AutomationViewModel : AsyncViewModel
{
    private readonly IDialogService? _dialogs;
    private readonly IFilePickerService? _filePicker;

    public ObservableCollection<AutomationItem> Automations { get; } = new();

    public ObservableCollection<AutomationItem> FilteredAutomations { get; } = new();

    [ObservableProperty]
    private AutomationItem? selectedAutomation;

    [ObservableProperty]
    private string name = "";

    [ObservableProperty]
    private string trigger = "";

    [ObservableProperty]
    private string condition = "";

    [ObservableProperty]
    private string action = "";

    [ObservableProperty]
    private string searchText = "";

    public bool HasNoItems => FilteredAutomations.Count == 0;

    public bool IsEditing => SelectedAutomation is not null;

    public string EditorTitle =>
        SelectedAutomation?.Name is { Length: > 0 } title ? title : "New Automation";

    public string EditorDescription => SelectedAutomation is null
        ? "Create an automatic package workflow."
        : "Editing a saved automation — changes apply when you save.";

    public AutomationViewModel()
    {
        Automations.CollectionChanged += (_, _) => ApplyFilter();
    }

    public AutomationViewModel(IDialogService dialogs, IFilePickerService filePicker)
        : this()
    {
        _dialogs = dialogs;
        _filePicker = filePicker;
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    partial void OnSelectedAutomationChanged(AutomationItem? value)
    {
        if (value is null)
        {
            Name = "";
            Trigger = "";
            Condition = "";
            Action = "";
        }
        else
        {
            Name = value.Name;
            Trigger = value.Trigger;
            Condition = value.Condition;
            Action = value.Action;
        }

        OnPropertyChanged(nameof(IsEditing));
        OnPropertyChanged(nameof(EditorTitle));
        OnPropertyChanged(nameof(EditorDescription));
    }

    [RelayCommand]
    private void New()
    {
        SelectedAutomation = null;
        Name = "";
        Trigger = "";
        Condition = "";
        Action = "";
    }

    [RelayCommand]
    private void Save()
    {
        if (string.IsNullOrWhiteSpace(Name))
            return;

        if (SelectedAutomation is { } existing)
        {
            existing.Name = Name.Trim();
            existing.Trigger = Trigger.Trim();
            existing.Condition = Condition.Trim();
            existing.Action = Action.Trim();
        }
        else
        {
            var created = new AutomationItem
            {
                Name = Name.Trim(),
                Trigger = Trigger.Trim(),
                Condition = Condition.Trim(),
                Action = Action.Trim()
            };

            Automations.Insert(0, created);
            SelectedAutomation = created;
        }

        OnPropertyChanged(nameof(EditorTitle));
    }

    [RelayCommand]
    private async Task Delete()
    {
        if (SelectedAutomation is not { } target)
            return;

        if (_dialogs is not null)
        {
            bool confirmed = await _dialogs.ConfirmAsync(
                "Delete Automation",
                $"Delete '{target.Name}'? This cannot be undone.");

            if (!confirmed)
                return;
        }

        Automations.Remove(target);
        SelectedAutomation = null;
    }

    [RelayCommand]
    private async Task Import()
    {
        if (_filePicker is null)
            return;

        var file = await _filePicker.PickOpenJsonAsync("Import Automations");

        if (file is null)
            return;

        try
        {
            string text = await File.ReadAllTextAsync(file.Path.LocalPath);

            var items = JsonSerializer.Deserialize<List<AutomationItem>>(text);

            if (items is null)
                return;

            foreach (var item in items)
            {
                item.Id = Guid.NewGuid();
                Automations.Add(item);
            }
        }
        catch (Exception ex)
        {
            if (_dialogs is not null)
                await _dialogs.ShowErrorAsync("Import Failed", ex.Message);
        }
    }

    [RelayCommand]
    private async Task Export()
    {
        if (_filePicker is null)
            return;

        var file = await _filePicker.PickSaveJsonAsync(
            "Export Automations",
            $"automations-{DateTime.Now:yyyyMMdd-HHmmss}.json");

        if (file is null)
            return;

        try
        {
            string json = JsonSerializer.Serialize(
                Automations.ToList(),
                new JsonSerializerOptions { WriteIndented = true });

            string? directory = Path.GetDirectoryName(file.Path.LocalPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            await File.WriteAllTextAsync(file.Path.LocalPath, json);
        }
        catch (Exception ex)
        {
            if (_dialogs is not null)
                await _dialogs.ShowErrorAsync("Export Failed", ex.Message);
        }
    }

    [RelayCommand]
    public Task Refresh()
    {
        return Task.CompletedTask;
    }

    public override Task InitializeAsync()
    {
        return Refresh();
    }

    private void ApplyFilter()
    {
        IEnumerable<AutomationItem> filtered = Automations;

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            filtered = filtered.Where(a =>
                a.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                || a.Trigger.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                || a.Action.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
        }

        var matches = filtered.ToList();

        if (SelectedAutomation is not null && !matches.Contains(SelectedAutomation))
            SelectedAutomation = null;

        FilteredAutomations.Clear();

        foreach (var item in matches)
            FilteredAutomations.Add(item);

        OnPropertyChanged(nameof(HasNoItems));
    }
}

