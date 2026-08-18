// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using WinGetXAvalonia.Core.Providers;
using WinGetXAvalonia.Core.State;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Dialogs;
using WinGetXAvalonia.Services.Operations;
using WinGetXAvalonia.Services.Workspaces;

namespace WinGetXAvalonia.ViewModels;

public partial class WorkspacesViewModel : AsyncViewModel
{
    private readonly IWorkspaceService _workspaces;
    private readonly IDialogService _dialogs;
    private readonly IFilePickerService _filePicker;
    private readonly IOperationService? _operations;
    private readonly IPackageProvider? _provider;

    private readonly List<WorkspaceModel> _allWorkspaces = new();
    private WorkspaceModel? _editing;
    private WorkspaceModel? _boundWorkspace;

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
    private string newPackageId = "";

    [ObservableProperty]
    private string newPackageName = "";

    [ObservableProperty]
    private bool isEditorOpen;

    [ObservableProperty]
    private bool isPackageEditorOpen;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(WorkspacesSummary))]
    private int workspaceCount;

    public bool IsEditing => _editing is not null;

    public bool IsNothingSelected => SelectedWorkspace is null;

    public bool HasPackages => SelectedWorkspace is { Packages.Count: > 0 };

    public string InstallAllLabel
    {
        get
        {
            var pending = SelectedWorkspace?.Packages
                .Count(p => p.InstallState != PackageInstallState.Installed) ?? 0;

            return pending == 0 ? "Install All" : $"Install All ({pending})";
        }
    }

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
        IFilePickerService filePicker,
        IOperationService? operations = null,
        IPackageProvider? provider = null)
    {
        _workspaces = workspaces;
        _dialogs = dialogs;
        _filePicker = filePicker;
        _operations = operations;
        _provider = provider;

        Workspaces.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(WorkspaceCount));
        };
    }

    public override async Task InitializeAsync()
    {
        await _workspaces.EnsureExampleWorkspacesAsync();
        await RefreshAsync();
    }

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

            if (SelectedWorkspace is null && Workspaces.Count > 0)
                SelectedWorkspace = Workspaces[0];

            if (SelectedWorkspace is not null)
            {
                SelectedWorkspace =
                    Workspaces.FirstOrDefault(x =>
                        x.Id == SelectedWorkspace.Id);
            }

            State = _allWorkspaces.Count == 0 ? LoadState.Empty : LoadState.Loaded;
        }
        catch (Exception ex)
        {
            State = LoadState.Error;
            ErrorMessage = ex.Message;
        }
    }

    partial void OnSelectedWorkspaceChanged(WorkspaceModel? value)
    {
        RebindPackageListeners(value);

        OnPropertyChanged(nameof(IsNothingSelected));
        OnPropertyChanged(nameof(HasPackages));
        OnPropertyChanged(nameof(InstallAllLabel));
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
    private void CancelWorkspaceEditor()
    {
        _editing = null;
        IsEditorOpen = false;
        NewWorkspaceName = "";
        NewWorkspaceDescription = "";

        NotifyEditorState();
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

        if (Workspaces.Count == 0)
        {
            SelectedWorkspace = null;
        }

        State = _allWorkspaces.Count == 0 ? LoadState.Empty : LoadState.Loaded;
    }

    [RelayCommand]
    private void OpenPackageEditor()
    {
        if (SelectedWorkspace is null)
            return;

        NewPackageId = "";
        NewPackageName = "";
        IsPackageEditorOpen = true;
    }

    [RelayCommand]
    private void CancelPackageEditor()
        => IsPackageEditorOpen = false;

    [RelayCommand]
    private async Task AddPackage()
    {
        if (SelectedWorkspace is null)
            return;

        var id = NewPackageId.Trim();

        if (string.IsNullOrWhiteSpace(id))
            return;

        if (SelectedWorkspace.Packages.Any(p =>
                string.Equals(p.Id, id, StringComparison.OrdinalIgnoreCase)))
            return;

        SelectedWorkspace.Packages.Add(new WorkspacePackage
        {
            Id = id,
            Name = string.IsNullOrWhiteSpace(NewPackageName) ? id : NewPackageName.Trim()
        });

        IsPackageEditorOpen = false;
        NewPackageId = "";
        NewPackageName = "";

        await PersistSelectedAsync();
    }

    [RelayCommand]
    private async Task RemovePackage(WorkspacePackage? package)
    {
        if (SelectedWorkspace is null || package is null)
            return;

        SelectedWorkspace.Packages.Remove(package);

        await PersistSelectedAsync();
    }

    [RelayCommand]
    private async Task InstallPackage(WorkspacePackage? package)
    {
        if (package is null || _operations is null)
            return;

        if (package.IsBusy || package.InstallState == PackageInstallState.Installed)
            return;

        package.InstallState = PackageInstallState.Installing;

        try
        {
            await _operations.InstallAsync(package.Id);
            package.InstallState = PackageInstallState.Installed;
        }
        catch
        {
            package.InstallState = PackageInstallState.NotInstalled;
        }
    }

    [RelayCommand]
    private async Task InstallAll()
    {
        if (SelectedWorkspace is null || _operations is null)
            return;

        var pending = SelectedWorkspace.Packages
            .Where(p => p.InstallState != PackageInstallState.Installed)
            .ToList();

        foreach (var package in pending)
            await InstallPackage(package);
    }

    [RelayCommand]
    private async Task RefreshInstallStates()
    {
        if (SelectedWorkspace is null || _provider is null)
            return;

        IReadOnlyList<InstalledPackage> installed;

        try
        {
            installed = await _provider.GetInstalledAsync();
        }
        catch
        {
            return;
        }

        var installedIds = installed
            .Where(x => !string.IsNullOrWhiteSpace(x.Id))
            .Select(x => x.Id)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var package in SelectedWorkspace.Packages)
        {
            package.InstallState = installedIds.Contains(package.Id)
                ? PackageInstallState.Installed
                : PackageInstallState.NotInstalled;
        }

        OnPropertyChanged(nameof(InstallAllLabel));
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

        if (SelectedWorkspace is not null &&
            !Workspaces.Contains(SelectedWorkspace))
        {
            SelectedWorkspace = null;
        }
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

        SelectedWorkspace = _editing;
    }

    private async Task UpdateWorkspaceAsync(string name)
    {
        _editing!.Name = name;
        _editing.Description = NewWorkspaceDescription.Trim();
        await _workspaces.SaveAsync(_editing);

        SelectedWorkspace = _editing;
    }

    private async Task PersistSelectedAsync()
    {
        if (SelectedWorkspace is null)
            return;

        try
        {
            await _workspaces.SaveAsync(SelectedWorkspace);
        }
        catch (Exception ex)
        {
            await _dialogs.ShowErrorAsync("Save Failed", ex.Message);
        }
    }

    private void RebindPackageListeners(WorkspaceModel? value)
    {
        if (_boundWorkspace == value)
            return;

        if (_boundWorkspace is not null)
        {
            _boundWorkspace.Packages.CollectionChanged -= OnPackagesCollectionChanged;

            foreach (var package in _boundWorkspace.Packages)
                package.PropertyChanged -= OnPackagePropertyChanged;
        }

        _boundWorkspace = value;

        if (value is not null)
        {
            value.Packages.CollectionChanged += OnPackagesCollectionChanged;

            foreach (var package in value.Packages)
                package.PropertyChanged += OnPackagePropertyChanged;
        }
    }

    private void OnPackagesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (WorkspacePackage package in e.OldItems)
                package.PropertyChanged -= OnPackagePropertyChanged;
        }

        if (e.NewItems is not null)
        {
            foreach (WorkspacePackage package in e.NewItems)
                package.PropertyChanged += OnPackagePropertyChanged;
        }

        OnPropertyChanged(nameof(HasPackages));
        OnPropertyChanged(nameof(InstallAllLabel));
    }

    private void OnPackagePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(WorkspacePackage.InstallState))
            OnPropertyChanged(nameof(InstallAllLabel));
    }
}

