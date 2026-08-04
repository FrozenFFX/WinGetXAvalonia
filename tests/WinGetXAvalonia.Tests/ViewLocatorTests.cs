// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Collections.ObjectModel;
using Avalonia.Threading;
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
    private static async Task<object> BuildAsync(object? dataContext)
        => await Dispatcher.UIThread.InvokeAsync(() => (object)new ViewLocator().Build(dataContext)!);

    [Theory]
    [InlineData(typeof(BlueprintsViewModel), typeof(BlueprintsView))]
    [InlineData(typeof(AutomationViewModel), typeof(AutomationView))]
    [InlineData(typeof(SecurityViewModel), typeof(SecurityView))]
    public async Task PlaceholderModules_ResolveToDedicatedView(Type viewModelType, Type expectedViewType)
    {
        var vm = (ViewModelBase)Activator.CreateInstance(viewModelType)!;
        Assert.IsType(expectedViewType, await BuildAsync(vm));
    }

    [Fact]
    public async Task WorkspacesViewModel_ResolvesToWorkspacesView()
    {
        var vm = new WorkspacesViewModel(
            new FakeWorkspaceService(),
            new FakeDialogService(),
            new FakeFilePickerService());

        Assert.IsType<WorkspacesView>(await BuildAsync(vm));
    }

    [Fact]
    public async Task HistoryViewModel_ResolvesToHistoryView()
    {
        var vm = new HistoryViewModel(new FakeHistoryService(), new FakeDialogService(), new FakeEventBus());

        Assert.IsType<HistoryView>(await BuildAsync(vm));
    }

    [Fact]
    public async Task DeveloperViewModel_ResolvesToDeveloperView()
    {
        var vm = new DeveloperViewModel(new QueuePanelViewModel(new FakeOperationQueue()));

        Assert.IsType<DeveloperView>(await BuildAsync(vm));
    }

    [Fact]
    public async Task StudioViewModel_ResolvesToStudioView()
    {
        var vm = new StudioViewModel(new FakeStudioService());

        Assert.IsType<StudioView>(await BuildAsync(vm));
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
