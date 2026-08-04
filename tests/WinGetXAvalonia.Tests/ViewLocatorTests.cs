using System.Collections.ObjectModel;
using WinGetXAvalonia.Core.Events;
using WinGetXAvalonia.Core.PackageEngine;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Dialogs;
using WinGetXAvalonia.Services.History;
using WinGetXAvalonia.Services.Queue;
using WinGetXAvalonia.Services.Studio;
using WinGetXAvalonia.Services.Workspaces;
using WinGetXAvalonia.ViewModels;
using WinGetXAvalonia.Views;

namespace WinGetXAvalonia.Tests;

public class ViewLocatorTests
{
    [Theory]
    [InlineData(typeof(BlueprintsViewModel))]
    [InlineData(typeof(AutomationViewModel))]
    [InlineData(typeof(SecurityViewModel))]
    public void PlaceholderModules_ResolveToPlaceholderView(Type viewModelType)
    {
        var vm = (ViewModelBase)Activator.CreateInstance(viewModelType)!;
        Assert.IsType<PlaceholderView>(new ViewLocator().Build(vm));
    }

    [Fact]
    public void WorkspacesViewModel_ResolvesToWorkspacesView()
    {
        var vm = new WorkspacesViewModel(
            new FakeWorkspaceService(),
            new FakeDialogService(),
            new FakeFilePickerService());

        Assert.IsType<WorkspacesView>(new ViewLocator().Build(vm));
    }

    [Fact]
    public void HistoryViewModel_ResolvesToHistoryView()
    {
        var vm = new HistoryViewModel(new FakeHistoryService(), new FakeDialogService(), new FakeEventBus());

        Assert.IsType<HistoryView>(new ViewLocator().Build(vm));
    }

    [Fact]
    public void DeveloperViewModel_ResolvesToDeveloperView()
    {
        var vm = new DeveloperViewModel(new QueuePanelViewModel(new FakeOperationQueue()));

        Assert.IsType<DeveloperView>(new ViewLocator().Build(vm));
    }

    [Fact]
    public void StudioViewModel_ResolvesToStudioView()
    {
        var vm = new StudioViewModel(new FakeStudioService());

        Assert.IsType<StudioView>(new ViewLocator().Build(vm));
    }

    private sealed class FakeWorkspaceService : IWorkspaceService
    {
        public Task<IReadOnlyList<WorkspaceModel>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<WorkspaceModel>>(new List<WorkspaceModel>());

        public Task<WorkspaceModel?> GetAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult<WorkspaceModel?>(null);

        public Task SaveAsync(WorkspaceModel workspace, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task ImportAsync(string file, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task ExportAsync(Guid id, string file, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeOperationQueue : IOperationQueue
    {
        public ObservableCollection<QueueItem> Items { get; } = new();

        public Task EnqueueAsync(PackageRequest request)
        {
            Items.Add(new QueueItem
            {
                Request = request,
                Status = QueueStatus.Waiting
            });
            return Task.CompletedTask;
        }

        public Task StartAsync() => Task.CompletedTask;

        public void Cancel(Guid id) { }

        public void ClearCompleted() { }
    }

    private sealed class FakeHistoryService : IHistoryService
    {
        public Task AddAsync(HistoryEntry entry) => Task.CompletedTask;

        public Task<IReadOnlyList<HistoryEntry>> GetAllAsync()
            => Task.FromResult<IReadOnlyList<HistoryEntry>>(new List<HistoryEntry>());

        public Task ClearAsync() => Task.CompletedTask;
    }

    private sealed class FakeStudioService : IStudioService
    {
        public Task<IReadOnlyList<StudioTool>> GetToolsAsync()
            => Task.FromResult<IReadOnlyList<StudioTool>>(new List<StudioTool>());
    }

    private sealed class FakeDialogService : IDialogService
    {
        public Task<bool> ConfirmAsync(string title, string message, string? confirmText = null, string? cancelText = null)
            => Task.FromResult(true);

        public Task ProgressAsync(string title, Func<IProgress<double>, Task> operation)
            => Task.CompletedTask;

        public Task ShowErrorAsync(string title, string message)
            => Task.CompletedTask;
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
}
