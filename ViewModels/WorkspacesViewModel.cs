// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using WinGetXAvalonia.Core.State;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Dialogs;
using WinGetXAvalonia.Services.Workspaces;

namespace WinGetXAvalonia.ViewModels;

public partial class WorkspacesViewModel : AsyncViewModel
{
    private readonly IWorkspaceService _workspaces;
    private readonly IDialogService _dialogs;
    private readonly IFilePickerService _filePicker;

    private readonly List<WorkspaceModel> _allWorkspaces = new();
    private WorkspaceModel? _editing;

    public ObservableCollection<WorkspaceModel> Workspaces { get; } = new();

    [ObservableProperty]
    private WorkspaceModel? selectedWorkspace;

    [ObservableProperty]
    private string searchText = "";

    [ObservableProperty]
    private string newWorkspaceName = "";

    [ObservableProperty]
    private string newWorkspaceDescription = "";

    [ObservableProperty]
    private bool isEditorOpen;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(WorkspacesSummary))]
    private int workspaceCount;

    public bool IsEditing => _editing is not null;

    public string EditorTitle => IsEditing ? "Edit Workspace" : "New Workspace";

    public string EditorAction => IsEditing ? "Save" : "Create";

    public string WorkspacesSummary => WorkspaceCount switch
    {
        0 => "No workspaces",
        1 => "1 workspace",
        var count => $"{count} workspaces"
    };

    public WorkspacesViewModel(
        IWorkspaceService workspaces,
        IDialogService dialogs,
        IFilePickerService filePicker)
    {
        _workspaces = workspaces;
        _dialogs = dialogs;
        _filePicker = filePicker;
    }

    public Task InitializeAsync()
        => RefreshAsync();

    partial void OnSearchTextChanged(string value)
        => ApplyFilter();

    [RelayCommand]
    public async Task RefreshAsync()
    {
        State = LoadState.Loading;
        ErrorMessage = null;

        try
        {
            var items = await _workspaces.GetAllAsync();

            _allWorkspaces.Clear();
            _allWorkspaces.AddRange(items.OrderByDescending(w => w.LastUsed ?? w.Modified));

            WorkspaceCount = _allWorkspaces.Count;

            ApplyFilter();

            State = _allWorkspaces.Count == 0 ? LoadState.Empty : LoadState.Loaded;
        }
        catch (Exception ex)
        {
            State = LoadState.Error;
            ErrorMessage = ex.Message;
        }
    }

    [RelayCommand]
    private void Open(WorkspaceModel? workspace)
    {
        if (workspace is null)
            return;

        SelectedWorkspace = workspace;
    }

    [RelayCommand]
    private void NewWorkspace()
    {
        _editing = null;
        NewWorkspaceName = "";
        NewWorkspaceDescription = "";
        IsEditorOpen = true;

        NotifyEditorState();
    }

    [RelayCommand]
    private void Edit(WorkspaceModel? workspace)
    {
        if (workspace is null)
            return;

        _editing = workspace;
        NewWorkspaceName = workspace.Name;
        NewWorkspaceDescription = workspace.Description;
        IsEditorOpen = true;

        NotifyEditorState();
    }

    [RelayCommand]
    private async Task SaveWorkspace()
    {
        var name = NewWorkspaceName.Trim();

        if (string.IsNullOrWhiteSpace(name))
            return;

        if (_editing is null)
            await CreateWorkspaceAsync(name);
        else
            await UpdateWorkspaceAsync(name);

        ApplyFilter();

        SelectedWorkspace = _editing;
        _editing = null;
        IsEditorOpen = false;
        NewWorkspaceName = "";
        NewWorkspaceDescription = "";

        NotifyEditorState();

        State = LoadState.Loaded;
    }

    [RelayCommand]
    private async Task DeleteWorkspace(WorkspaceModel? workspace)
    {
        if (workspace is null)
            return;

        bool confirmed = await _dialogs.ConfirmAsync(
            "Delete Workspace",
            $"Delete \"{workspace.Name}\"?\n\nThis action cannot be undone.",
            "Delete");

        if (!confirmed)
            return;

        await _workspaces.DeleteAsync(workspace.Id);

        _allWorkspaces.Remove(workspace);
        WorkspaceCount = _allWorkspaces.Count;

        if (SelectedWorkspace == workspace)
            SelectedWorkspace = null;

        ApplyFilter();

        State = _allWorkspaces.Count == 0 ? LoadState.Empty : LoadState.Loaded;
    }

    [RelayCommand]
    private async Task Import()
    {
        var file = await _filePicker.PickOpenJsonAsync("Import Workspace");

        if (file is null)
            return;

        try
        {
            await _workspaces.ImportAsync(file.Path.LocalPath);

            await RefreshAsync();
        }
        catch (Exception ex)
        {
            await _dialogs.ShowErrorAsync("Import Failed", ex.Message);
        }
    }

    [RelayCommand]
    private async Task Export(WorkspaceModel? workspace)
    {
        if (workspace is null)
            return;

        var file = await _filePicker.PickSaveJsonAsync(
            "Export Workspace",
            $"{workspace.Name}-{DateTime.Now:yyyyMMdd-HHmmss}.json");

        if (file is null)
            return;

        try
        {
            await _workspaces.ExportAsync(workspace.Id, file.Path.LocalPath);
        }
        catch (Exception ex)
        {
            await _dialogs.ShowErrorAsync("Export Failed", ex.Message);
        }
    }

    private void ApplyFilter()
    {
        IEnumerable<WorkspaceModel> filtered = _allWorkspaces;

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            filtered = filtered.Where(w =>
                w.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                || w.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
        }

        Workspaces.Clear();

        foreach (var workspace in filtered)
            Workspaces.Add(workspace);
    }

    private void NotifyEditorState()
    {
        OnPropertyChanged(nameof(IsEditing));
        OnPropertyChanged(nameof(EditorTitle));
        OnPropertyChanged(nameof(EditorAction));
    }

    private async Task CreateWorkspaceAsync(string name)
    {
        _editing = new WorkspaceModel
        {
            Name = name,
            Description = NewWorkspaceDescription.Trim()
        };

        await _workspaces.SaveAsync(_editing);
        _allWorkspaces.Insert(0, _editing);
        WorkspaceCount = _allWorkspaces.Count;
    }

    private async Task UpdateWorkspaceAsync(string name)
    {
        _editing!.Name = name;
        _editing.Description = NewWorkspaceDescription.Trim();
        await _workspaces.SaveAsync(_editing);
    }
}
