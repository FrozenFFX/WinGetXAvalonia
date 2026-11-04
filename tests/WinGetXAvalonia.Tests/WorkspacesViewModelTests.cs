// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Collections.ObjectModel;
using WinGetXAvalonia.Core.Operations;
using WinGetXAvalonia.Core.Providers;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Dialogs;
using WinGetXAvalonia.Services.Operations;
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

        public int EnsureCalls { get; private set; }

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

        public Task ImportJsonAsync(string json, CancellationToken cancellationToken = default)
        {
            Imported.Add(json);
            return Task.CompletedTask;
        }

        public Task ExportAsync(Guid id, string file, CancellationToken cancellationToken = default)
        {
            Exported.Add((id, file));
            return Task.CompletedTask;
        }

        public Task EnsureExampleWorkspacesAsync(CancellationToken cancellationToken = default)
        {
            EnsureCalls++;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeOperationService : IOperationService
    {
        public List<string> Installed { get; } = new();

        public Task InstallAsync(string packageId)
        {
            Installed.Add(packageId);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(string packageId) => Task.CompletedTask;

        public Task RemoveAsync(string packageId) => Task.CompletedTask;

        public Task<BulkUpdateResult> UpgradeAllAsync(
            IProgress<BulkUpdateProgress>? progress = null,
            CancellationToken ct = default)
            => Task.FromResult(BulkUpdateResult.None);

        public Task<BulkUpdateResult> UpdateAllAsync(
            IReadOnlyList<string> packageIds,
            IProgress<BulkUpdateProgress>? progress = null,
            CancellationToken ct = default)
            => Task.FromResult(BulkUpdateResult.None);
    }

    private sealed class FakeProvider : IPackageProvider
    {
        public List<InstalledPackage> Installed { get; } = new();

        public Task<IReadOnlyList<PackageModel>> SearchAsync(string query)
            => Task.FromResult<IReadOnlyList<PackageModel>>(Array.Empty<PackageModel>());

        public Task<IReadOnlyList<InstalledPackage>> GetInstalledAsync()
            => Task.FromResult<IReadOnlyList<InstalledPackage>>(Installed);

        public Task<IReadOnlyList<UpdatePackage>> GetUpdatesAsync()
            => Task.FromResult<IReadOnlyList<UpdatePackage>>(Array.Empty<UpdatePackage>());

        public Task InstallAsync(string id, IOperationProgress? progress = null)
            => Task.CompletedTask;

        public Task UpgradeAsync(string id, IOperationProgress? progress = null)
            => Task.CompletedTask;

        public Task UninstallAsync(string id, IOperationProgress? progress = null)
            => Task.CompletedTask;

        public Task<string?> GetHomepageAsync(string id)
            => Task.FromResult<string?>(null);

        public Task<PackageDetails> GetDetailsAsync(string id)
            => Task.FromResult(new PackageDetails());
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
        FakeDialogService? dialogs = null,
        FakeOperationService? operations = null,
        FakeProvider? provider = null)
        => new(
            service ?? new FakeWorkspaceService(),
            dialogs ?? new FakeDialogService(),
            new FakeFilePickerService(),
            operations,
            provider);

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

    [Fact]
    public async Task InitializeAsync_EnsuresExampleWorkspaces()
    {
        var service = new FakeWorkspaceService();
        var vm = Create(service);

        await vm.InitializeAsync();

        Assert.Equal(1, service.EnsureCalls);
    }

    [Fact]
    public async Task InstallAll_InstallsOnlyPendingPackages()
    {
        var service = new FakeWorkspaceService();
        var operations = new FakeOperationService();
        var workspace = new WorkspaceModel { Name = "Dev" };
        workspace.Packages.Add(new WorkspacePackage { Id = "Git.Git", Name = "Git" });
        workspace.Packages.Add(new WorkspacePackage { Id = "Microsoft.VisualStudioCode", Name = "VS Code" });
        workspace.Packages[1].InstallState = PackageInstallState.Installed;
        service.Items.Add(workspace);

        var vm = Create(service, operations: operations);
        await vm.InitializeAsync();

        await vm.InstallAllCommand.ExecuteAsync(null);

        Assert.Equal(new[] { "Git.Git" }, operations.Installed);
        Assert.Equal(PackageInstallState.Installed, workspace.Packages[0].InstallState);
    }

    [Fact]
    public async Task InstallAll_NothingPending_DoesNothing()
    {
        var service = new FakeWorkspaceService();
        var operations = new FakeOperationService();
        var workspace = new WorkspaceModel { Name = "Dev" };
        workspace.Packages.Add(new WorkspacePackage
        {
            Id = "Git.Git",
            Name = "Git",
            InstallState = PackageInstallState.Installed
        });
        service.Items.Add(workspace);

        var vm = Create(service, operations: operations);
        await vm.InitializeAsync();

        await vm.InstallAllCommand.ExecuteAsync(null);

        Assert.Empty(operations.Installed);
    }

    [Fact]
    public async Task InstallPackage_UpdatesStateAfterSuccess()
    {
        var service = new FakeWorkspaceService();
        var operations = new FakeOperationService();
        var workspace = new WorkspaceModel { Name = "Dev" };
        workspace.Packages.Add(new WorkspacePackage { Id = "Git.Git", Name = "Git" });
        service.Items.Add(workspace);

        var vm = Create(service, operations: operations);
        await vm.InitializeAsync();

        await vm.InstallPackageCommand.ExecuteAsync(workspace.Packages[0]);

        Assert.Contains("Git.Git", operations.Installed);
        Assert.Equal(PackageInstallState.Installed, workspace.Packages[0].InstallState);
    }

    [Fact]
    public async Task InstallPackage_NoOperationService_DoesNothing()
    {
        var service = new FakeWorkspaceService();
        var workspace = new WorkspaceModel { Name = "Dev" };
        workspace.Packages.Add(new WorkspacePackage { Id = "Git.Git", Name = "Git" });
        service.Items.Add(workspace);

        var vm = Create(service);
        await vm.InitializeAsync();

        await vm.InstallPackageCommand.ExecuteAsync(workspace.Packages[0]);

        Assert.Equal(PackageInstallState.NotInstalled, workspace.Packages[0].InstallState);
    }

    [Fact]
    public async Task InstallAllLabel_ReflectsPendingCount()
    {
        var service = new FakeWorkspaceService();
        var operations = new FakeOperationService();
        var workspace = new WorkspaceModel { Name = "Dev" };
        workspace.Packages.Add(new WorkspacePackage { Id = "Git.Git", Name = "Git" });
        workspace.Packages.Add(new WorkspacePackage
        {
            Id = "Docker.DockerDesktop",
            Name = "Docker",
            InstallState = PackageInstallState.Installed
        });
        service.Items.Add(workspace);

        var vm = Create(service, operations: operations);
        await vm.InitializeAsync();

        Assert.Equal("Install All (1)", vm.InstallAllLabel);

        await vm.InstallAllCommand.ExecuteAsync(null);

        Assert.Equal("Install All", vm.InstallAllLabel);
    }

    [Fact]
    public async Task AddPackage_AddsAndPersists()
    {
        var service = new FakeWorkspaceService();
        var workspace = new WorkspaceModel { Name = "Dev" };
        service.Items.Add(workspace);

        var vm = Create(service);
        await vm.InitializeAsync();

        vm.OpenPackageEditorCommand.Execute(null);
        Assert.True(vm.IsPackageEditorOpen);

        vm.NewPackageId = "Git.Git";
        vm.NewPackageName = "Git";
        await vm.AddPackageCommand.ExecuteAsync(null);

        Assert.False(vm.IsPackageEditorOpen);
        Assert.Single(workspace.Packages);
        Assert.Equal("Git.Git", workspace.Packages[0].Id);
        Assert.Equal("Git", workspace.Packages[0].Name);
        Assert.Equal(1, workspace.PackageCount);
    }

    [Fact]
    public async Task AddPackage_BlankId_KeepsEditorOpen()
    {
        var service = new FakeWorkspaceService();
        var workspace = new WorkspaceModel { Name = "Dev" };
        service.Items.Add(workspace);

        var vm = Create(service);
        await vm.InitializeAsync();

        vm.OpenPackageEditorCommand.Execute(null);
        vm.NewPackageId = "   ";
        await vm.AddPackageCommand.ExecuteAsync(null);

        Assert.Empty(workspace.Packages);
        Assert.True(vm.IsPackageEditorOpen);
    }

    [Fact]
    public async Task AddPackage_DuplicateId_Ignored()
    {
        var service = new FakeWorkspaceService();
        var workspace = new WorkspaceModel { Name = "Dev" };
        workspace.Packages.Add(new WorkspacePackage { Id = "Git.Git", Name = "Git" });
        service.Items.Add(workspace);

        var vm = Create(service);
        await vm.InitializeAsync();

        vm.OpenPackageEditorCommand.Execute(null);
        vm.NewPackageId = "git.git";
        await vm.AddPackageCommand.ExecuteAsync(null);

        Assert.Single(workspace.Packages);
    }

    [Fact]
    public async Task RemovePackage_RemovesAndPersists()
    {
        var service = new FakeWorkspaceService();
        var workspace = new WorkspaceModel { Name = "Dev" };
        workspace.Packages.Add(new WorkspacePackage { Id = "Git.Git", Name = "Git" });
        workspace.Packages.Add(new WorkspacePackage { Id = "Docker.DockerDesktop", Name = "Docker" });
        service.Items.Add(workspace);

        var vm = Create(service);
        await vm.InitializeAsync();

        await vm.RemovePackageCommand.ExecuteAsync(workspace.Packages[0]);

        Assert.Single(workspace.Packages);
        Assert.Equal("Docker.DockerDesktop", workspace.Packages[0].Id);
        Assert.Equal(1, workspace.PackageCount);
        Assert.True(vm.HasPackages);
    }

    [Fact]
    public async Task RefreshInstallStates_MarksInstalledPackages()
    {
        var service = new FakeWorkspaceService();
        var provider = new FakeProvider();
        provider.Installed.Add(new InstalledPackage { Id = "Git.Git", Version = "2.45.0" });

        var workspace = new WorkspaceModel { Name = "Dev" };
        workspace.Packages.Add(new WorkspacePackage { Id = "Git.Git", Name = "Git" });
        workspace.Packages.Add(new WorkspacePackage { Id = "Docker.DockerDesktop", Name = "Docker" });
        service.Items.Add(workspace);

        var vm = Create(service, provider: provider);
        await vm.InitializeAsync();

        await vm.RefreshInstallStatesCommand.ExecuteAsync(null);

        Assert.Equal(PackageInstallState.Installed, workspace.Packages[0].InstallState);
        Assert.Equal(PackageInstallState.NotInstalled, workspace.Packages[1].InstallState);
        Assert.Equal("Install All (1)", vm.InstallAllLabel);
    }
}
