using WinGetXAvalonia.Core.Events;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Dialogs;
using WinGetXAvalonia.Services.History;
using WinGetXAvalonia.Services.Operations;
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
        var vm = new WorkspacesViewModel(new FakeWorkspaceService(), new FakeDialogService());

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

    private sealed class FakeWorkspaceService : IWorkspaceService
    {
        public Task<IReadOnlyList<WorkspaceModel>> GetAllAsync()
            => Task.FromResult<IReadOnlyList<WorkspaceModel>>(new List<WorkspaceModel>());

        public Task SaveAsync(WorkspaceModel workspace) => Task.CompletedTask;

        public Task DeleteAsync(Guid id) => Task.CompletedTask;

        public Task ImportAsync(string file) => Task.CompletedTask;

        public Task ExportAsync(Guid id, string file) => Task.CompletedTask;
    }

    private sealed class FakeOperationQueue : IOperationQueue
    {
        public List<OperationItem> Items { get; } = new();

        public IReadOnlyList<OperationItem> Operations => Items;

        public event EventHandler? QueueChanged;

        public Task EnqueueAsync(OperationItem item, Func<Task> action)
        {
            QueueChanged?.Invoke(this, EventArgs.Empty);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeHistoryService : IHistoryService
    {
        public Task AddAsync(HistoryEntry entry) => Task.CompletedTask;

        public Task<IReadOnlyList<HistoryEntry>> GetAllAsync()
            => Task.FromResult<IReadOnlyList<HistoryEntry>>(new List<HistoryEntry>());

        public Task ClearAsync() => Task.CompletedTask;
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
}
