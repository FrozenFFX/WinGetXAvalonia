// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Collections.ObjectModel;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Dialogs;
using WinGetXAvalonia.Services.Workspaces;
using WinGetXAvalonia.ViewModels;

namespace WinGetXAvalonia.Tests;

public class WorkspacesViewModelTests
{
    private sealed class FakeWorkspaceService : IWorkspaceService
    {
        public List<WorkspaceModel> Items { get; } = new();

        public List<Guid> Deleted { get; } = new();

        public List<string> Imported { get; } = new();

        public List<(Guid Id, string File)> Exported { get; } = new();

        public bool ThrowOnGetAll { get; set; }

        public Task<IReadOnlyList<WorkspaceModel>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            if (ThrowOnGetAll)
                throw new InvalidOperationException("boom");

            return Task.FromResult<IReadOnlyList<WorkspaceModel>>(Items.ToList());
        }

        public Task<WorkspaceModel?> GetAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult<WorkspaceModel?>(Items.SingleOrDefault(i => i.Id == id));

        public Task SaveAsync(WorkspaceModel workspace, CancellationToken cancellationToken = default)
        {
            Items.RemoveAll(i => i.Id == workspace.Id);
            Items.Add(workspace);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            Deleted.Add(id);
            Items.RemoveAll(i => i.Id == id);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(Items.Any(i => i.Id == id));

        public Task ImportAsync(string file, CancellationToken cancellationToken = default)
        {
            Imported.Add(file);
            return Task.CompletedTask;
        }

        public Task ExportAsync(Guid id, string file, CancellationToken cancellationToken = default)
        {
            Exported.Add((id, file));
            return Task.CompletedTask;
        }
    }

    private sealed class FakeDialogService : IDialogService
    {
        public bool ConfirmResult { get; set; } = true;

        public List<string> Errors { get; } = new();

        public Task<bool> ConfirmAsync(
            string title,
            string message,
            string? confirmText = null,
            string? cancelText = null)
            => Task.FromResult(ConfirmResult);

        public Task ProgressAsync(string title, Func<IProgress<double>, Task> operation)
            => operation(new Progress<double>());

        public Task ShowErrorAsync(string title, string message)
        {
            Errors.Add(title);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeFilePickerService : IFilePickerService
    {
        public Task<Avalonia.Platform.Storage.IStorageFile?> PickOpenJsonAsync(
            string title,
            CancellationToken cancellationToken = default)
            => Task.FromResult<Avalonia.Platform.Storage.IStorageFile?>(null);

        public Task<Avalonia.Platform.Storage.IStorageFile?> PickSaveJsonAsync(
            string title,
            string suggestedFileName,
            CancellationToken cancellationToken = default)
            => Task.FromResult<Avalonia.Platform.Storage.IStorageFile?>(null);

        public Task<Avalonia.Platform.Storage.IStorageFolder?> PickFolderAsync(
            string title,
            CancellationToken cancellationToken = default)
            => Task.FromResult<Avalonia.Platform.Storage.IStorageFolder?>(null);
    }

    private static WorkspacesViewModel Create(
        FakeWorkspaceService? service = null,
        FakeDialogService? dialogs = null)
        => new(
            service ?? new FakeWorkspaceService(),
            dialogs ?? new FakeDialogService(),
            new FakeFilePickerService());

    [Fact]
    public async Task Load_PopulatesWorkspaces_NewestFirst()
    {
        var service = new FakeWorkspaceService();
        service.Items.Add(new WorkspaceModel { Name = "Older", Modified = DateTime.Now.AddDays(-1) });
        service.Items.Add(new WorkspaceModel { Name = "Newer", Modified = DateTime.Now });

        var vm = Create(service);
        await vm.InitializeAsync();

        Assert.Equal(2, vm.Workspaces.Count);
        Assert.Equal("Newer", vm.Workspaces[0].Name);
        Assert.True(vm.IsLoaded);
        Assert.Equal("2 workspaces", vm.WorkspacesSummary);
    }

    [Fact]
    public async Task Load_Empty_SetsEmptyState()
    {
        var vm = Create(new FakeWorkspaceService());
        await vm.InitializeAsync();

        Assert.True(vm.IsEmpty);
        Assert.Equal("No workspaces", vm.WorkspacesSummary);
    }

    [Fact]
    public async Task Load_Error_SetsErrorState()
    {
        var service = new FakeWorkspaceService { ThrowOnGetAll = true };
        var vm = Create(service);

        await vm.InitializeAsync();

        Assert.True(vm.HasError);
        Assert.Equal("boom", vm.ErrorMessage);
    }

    [Fact]
    public async Task Create_AddsAndPersists()
    {
        var service = new FakeWorkspaceService();
        var vm = Create(service);

        vm.NewWorkspaceName = "  Gaming  ";
        await vm.SaveWorkspaceCommand.ExecuteAsync(null);

        Assert.Single(vm.Workspaces);
        Assert.Equal("Gaming", vm.Workspaces[0].Name);
        Assert.Single(service.Items);
        Assert.Same(vm.Workspaces[0], vm.SelectedWorkspace);
        Assert.False(vm.IsEditorOpen);
        Assert.Equal("", vm.NewWorkspaceName);
        Assert.True(vm.IsLoaded);
    }

    [Fact]
    public async Task Create_BlankName_DoesNothing()
    {
        var service = new FakeWorkspaceService();
        var vm = Create(service);

        vm.NewWorkspaceName = "   ";
        await vm.SaveWorkspaceCommand.ExecuteAsync(null);

        Assert.Empty(vm.Workspaces);
        Assert.Empty(service.Items);
    }

    [Fact]
    public async Task Delete_Confirmed_RemovesAndPersists()
    {
        var service = new FakeWorkspaceService();
        service.Items.Add(new WorkspaceModel { Name = "Dev" });
        var vm = Create(service);
        await vm.InitializeAsync();

        var target = vm.Workspaces[0];
        await vm.DeleteWorkspaceCommand.ExecuteAsync(target);

        Assert.Contains(target.Id, service.Deleted);
        Assert.Empty(vm.Workspaces);
        Assert.True(vm.IsEmpty);
    }

    [Fact]
    public async Task Delete_Cancelled_KeepsWorkspace()
    {
        var service = new FakeWorkspaceService();
        service.Items.Add(new WorkspaceModel { Name = "Dev" });
        var dialogs = new FakeDialogService { ConfirmResult = false };
        var vm = Create(service, dialogs);
        await vm.InitializeAsync();

        await vm.DeleteWorkspaceCommand.ExecuteAsync(vm.Workspaces[0]);

        Assert.Empty(service.Deleted);
        Assert.Single(vm.Workspaces);
    }

    [Fact]
    public async Task Search_FiltersWorkspaces_ByName()
    {
        var service = new FakeWorkspaceService();
        service.Items.Add(new WorkspaceModel { Name = "Gaming" });
        service.Items.Add(new WorkspaceModel { Name = "Work" });
        var vm = Create(service);
        await vm.InitializeAsync();

        vm.SearchText = "gaming";

        Assert.Single(vm.Workspaces);
        Assert.Equal("Gaming", vm.Workspaces[0].Name);
        Assert.Equal(2, vm.WorkspaceCount);
    }

    [Fact]
    public async Task Search_MatchesDescription()
    {
        var service = new FakeWorkspaceService();
        service.Items.Add(new WorkspaceModel
        {
            Name = "Dev",
            Description = "Node and database setup"
        });
        service.Items.Add(new WorkspaceModel { Name = "Gaming" });
        var vm = Create(service);
        await vm.InitializeAsync();

        vm.SearchText = "database";
        Assert.Single(vm.Workspaces);
        Assert.Equal("Dev", vm.Workspaces[0].Name);

    }

    [Fact]
    public async Task Search_NoMatch_ShowsEmptyListButKeepsLoaded()
    {
        var service = new FakeWorkspaceService();
        service.Items.Add(new WorkspaceModel { Name = "Gaming" });
        var vm = Create(service);
        await vm.InitializeAsync();

        vm.SearchText = "nothing matches";

        Assert.Empty(vm.Workspaces);
        Assert.True(vm.IsLoaded);
    }

    [Fact]
    public async Task Search_Cleared_ShowsAll()
    {
        var service = new FakeWorkspaceService();
        service.Items.Add(new WorkspaceModel { Name = "Gaming" });
        service.Items.Add(new WorkspaceModel { Name = "Work" });
        var vm = Create(service);
        await vm.InitializeAsync();

        vm.SearchText = "gaming";
        Assert.Single(vm.Workspaces);

        vm.SearchText = "";
        Assert.Equal(2, vm.Workspaces.Count);
    }

    [Fact]
    public async Task Open_SelectsWorkspace()
    {
        var service = new FakeWorkspaceService();
        service.Items.Add(new WorkspaceModel { Name = "Dev" });
        var vm = Create(service);
        await vm.InitializeAsync();

        vm.OpenCommand.Execute(vm.Workspaces[0]);

        Assert.Same(vm.Workspaces[0], vm.SelectedWorkspace);
    }

    [Fact]
    public async Task NewWorkspace_OpensCreateMode()
    {
        var vm = Create(new FakeWorkspaceService());
        await vm.InitializeAsync();

        vm.NewWorkspaceCommand.Execute(null);

        Assert.True(vm.IsEditorOpen);
        Assert.False(vm.IsEditing);
        Assert.Equal("New Workspace", vm.EditorTitle);
        Assert.Equal("Create", vm.EditorAction);
        Assert.Equal("", vm.NewWorkspaceName);
    }

    [Fact]
    public async Task Edit_PrefillsAndSavesChanges()
    {
        var service = new FakeWorkspaceService();
        var original = new WorkspaceModel { Name = "Dev", Description = "Old tools" };
        service.Items.Add(original);
        var vm = Create(service);
        await vm.InitializeAsync();

        vm.EditCommand.Execute(vm.Workspaces[0]);

        Assert.True(vm.IsEditorOpen);
        Assert.True(vm.IsEditing);
        Assert.Equal("Edit Workspace", vm.EditorTitle);
        Assert.Equal("Save", vm.EditorAction);
        Assert.Equal("Dev", vm.NewWorkspaceName);
        Assert.Equal("Old tools", vm.NewWorkspaceDescription);

        vm.NewWorkspaceName = "Development";
        vm.NewWorkspaceDescription = "New tools";
        await vm.SaveWorkspaceCommand.ExecuteAsync(null);

        Assert.False(vm.IsEditorOpen);
        Assert.Equal("Development", original.Name);
        Assert.Equal("New tools", original.Description);
        Assert.Same(original, vm.SelectedWorkspace);
        Assert.Single(service.Items);
    }

    [Fact]
    public async Task Create_OpensEditor_SavesNewWorkspace()
    {
        var service = new FakeWorkspaceService();
        var vm = Create(service);
        await vm.InitializeAsync();

        vm.NewWorkspaceCommand.Execute(null);
        vm.NewWorkspaceName = "Gaming";
        await vm.SaveWorkspaceCommand.ExecuteAsync(null);

        Assert.False(vm.IsEditorOpen);
        Assert.Single(vm.Workspaces);
        Assert.Equal("Gaming", vm.Workspaces[0].Name);
        Assert.Single(service.Items);
    }
}
