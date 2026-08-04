using System.Text.Json;
using WinGetXAvalonia.Core.Events;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Dialogs;
using WinGetXAvalonia.Services.History;
using WinGetXAvalonia.ViewModels;
namespace WinGetXAvalonia.Tests;

public class HistoryViewModelTests
{
    private sealed class FakeHistoryService : IHistoryService
    {
        public List<HistoryEntry> Entries { get; } = new();

        public int ClearCalls { get; private set; }

        public Task AddAsync(HistoryEntry entry)
        {
            Entries.Add(entry);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<HistoryEntry>> GetAllAsync()
            => Task.FromResult<IReadOnlyList<HistoryEntry>>(Entries.ToList());

        public Task ClearAsync()
        {
            ClearCalls++;
            Entries.Clear();
            return Task.CompletedTask;
        }
    }

    private sealed class FakeDialogService : IDialogService
    {
        public bool ConfirmResult { get; set; } = true;

        public int ConfirmCalls { get; private set; }

        public Task<bool> ConfirmAsync(string title, string message, string? confirmText = null, string? cancelText = null)
        {
            ConfirmCalls++;
            return Task.FromResult(ConfirmResult);
        }

        public Task ProgressAsync(string title, Func<IProgress<double>, Task> operation)
            => Task.CompletedTask;

        public Task ShowErrorAsync(string title, string message)
            => Task.CompletedTask;
    }

    private static (HistoryViewModel vm, FakeHistoryService history, FakeDialogService dialogs, FakeEventBus events) Create()
    {
        var history = new FakeHistoryService();
        var dialogs = new FakeDialogService();
        var events = new FakeEventBus();
        var vm = new HistoryViewModel(history, dialogs, events);
        return (vm, history, dialogs, events);
    }

    private static HistoryEntry Entry(
        string operation = "Install",
        string id = "Git.Git",
        string name = "Git",
        bool success = true,
        string? message = null,
        DateTime? timestamp = null)
        => new()
        {
            Timestamp = timestamp ?? DateTime.Now,
            Operation = operation,
            PackageId = id,
            PackageName = name,
            Version = "2.51.0",
            Success = success,
            Message = message
        };

    [Fact]
    public async Task Refresh_GroupsEntriesIntoTimeWindows()
    {
        var (vm, history, _, _) = Create();

        history.Entries.Add(Entry(timestamp: DateTime.Now));
        history.Entries.Add(Entry(timestamp: DateTime.Now.AddDays(-1)));
        history.Entries.Add(Entry(timestamp: DateTime.Now.AddDays(-3)));
        history.Entries.Add(Entry(timestamp: DateTime.Now.AddDays(-20)));

        await vm.RefreshCommand.ExecuteAsync(null);

        Assert.Equal(new[] { "Today", "Yesterday", "Last 7 Days", "Older" },
            vm.Groups.Select(g => g.Title));
        Assert.Equal(4, vm.OperationsCount);
        Assert.Equal("4 operations", vm.OperationsSummary);
    }

    [Fact]
    public async Task Refresh_OrdersEntriesNewestFirst()
    {
        var (vm, history, _, _) = Create();

        history.Entries.Add(Entry(timestamp: DateTime.Now.AddHours(-3)));
        history.Entries.Add(Entry(timestamp: DateTime.Now.AddHours(-1)));
        history.Entries.Add(Entry(timestamp: DateTime.Now.AddHours(-2)));

        await vm.RefreshCommand.ExecuteAsync(null);

        var items = vm.Groups.Single(g => g.Title == "Today").Items;
        Assert.True(items[0].Entry.Timestamp > items[1].Entry.Timestamp);
        Assert.True(items[1].Entry.Timestamp > items[2].Entry.Timestamp);
    }

    [Fact]
    public async Task Refresh_NoEntries_SetsEmptyState()
    {
        var (vm, _, _, _) = Create();

        await vm.RefreshCommand.ExecuteAsync(null);

        Assert.Empty(vm.Groups);
        Assert.True(vm.IsEmpty);
        Assert.Equal("No operations yet", vm.OperationsSummary);
    }

    [Fact]
    public async Task Filter_Installed_OnlyShowsInstallOperations()
    {
        var (vm, history, _, _) = Create();

        history.Entries.Add(Entry(operation: "Install", id: "a"));
        history.Entries.Add(Entry(operation: "Update", id: "b"));
        history.Entries.Add(Entry(operation: "Remove", id: "c"));

        await vm.RefreshCommand.ExecuteAsync(null);

        vm.SelectedFilter = HistoryFilterMode.Installed;

        var ids = vm.Groups.SelectMany(g => g.Items).Select(i => i.PackageId);
        Assert.Equal(new[] { "a" }, ids);
    }

    [Fact]
    public async Task Filter_Updated_OnlyShowsUpdateOperations()
    {
        var (vm, history, _, _) = Create();

        history.Entries.Add(Entry(operation: "Install", id: "a"));
        history.Entries.Add(Entry(operation: "Update", id: "b"));
        history.Entries.Add(Entry(operation: "Remove", id: "c"));

        await vm.RefreshCommand.ExecuteAsync(null);

        vm.SelectedFilter = HistoryFilterMode.Updated;

        var ids = vm.Groups.SelectMany(g => g.Items).Select(i => i.PackageId);
        Assert.Equal(new[] { "b" }, ids);
    }

    [Fact]
    public async Task Filter_Removed_OnlyShowsRemoveOperations()
    {
        var (vm, history, _, _) = Create();

        history.Entries.Add(Entry(operation: "Install", id: "a"));
        history.Entries.Add(Entry(operation: "Remove", id: "c"));

        await vm.RefreshCommand.ExecuteAsync(null);

        vm.SelectedFilter = HistoryFilterMode.Removed;

        var ids = vm.Groups.SelectMany(g => g.Items).Select(i => i.PackageId);
        Assert.Equal(new[] { "c" }, ids);
    }

    [Fact]
    public async Task Filter_Failed_OnlyShowsFailedEntries()
    {
        var (vm, history, _, _) = Create();

        history.Entries.Add(Entry(operation: "Install", id: "a", success: true));
        history.Entries.Add(Entry(operation: "Install", id: "b", success: false));
        history.Entries.Add(Entry(operation: "Update", id: "c", success: false));

        await vm.RefreshCommand.ExecuteAsync(null);

        vm.SelectedFilter = HistoryFilterMode.Failed;

        var ids = vm.Groups.SelectMany(g => g.Items).Select(i => i.PackageId).OrderBy(x => x);
        Assert.Equal(new[] { "b", "c" }, ids);
    }

    [Fact]
    public async Task Filter_All_ShowsEverything()
    {
        var (vm, history, _, _) = Create();

        history.Entries.Add(Entry(operation: "Install", id: "a"));
        history.Entries.Add(Entry(operation: "Update", id: "b", success: false));

        await vm.RefreshCommand.ExecuteAsync(null);

        Assert.Equal(2, vm.Groups.SelectMany(g => g.Items).Count());
    }

    [Fact]
    public async Task Search_MatchesPackageName_IdAndOperation()
    {
        var (vm, history, _, _) = Create();

        history.Entries.Add(Entry(name: "Git", id: "Git.Git", operation: "Install"));
        history.Entries.Add(Entry(name: "Visual Studio Code", id: "Microsoft.VisualStudioCode", operation: "Update"));
        history.Entries.Add(Entry(name: "Docker Desktop", id: "Docker.DockerDesktop", operation: "Remove"));

        await vm.RefreshCommand.ExecuteAsync(null);

        vm.SearchText = "git";
        Assert.Equal(new[] { "Git.Git" },
            vm.Groups.SelectMany(g => g.Items).Select(i => i.PackageId));

        vm.SearchText = "code";
        Assert.Equal(new[] { "Microsoft.VisualStudioCode" },
            vm.Groups.SelectMany(g => g.Items).Select(i => i.PackageId));

        vm.SearchText = "remove";
        Assert.Equal(new[] { "Docker.DockerDesktop" },
            vm.Groups.SelectMany(g => g.Items).Select(i => i.PackageId));
    }

    [Fact]
    public async Task Search_NoMatch_ShowsNoGroups()
    {
        var (vm, history, _, _) = Create();

        history.Entries.Add(Entry(name: "Git", id: "Git.Git"));

        await vm.RefreshCommand.ExecuteAsync(null);

        vm.SearchText = "zzz-no-match";

        Assert.Empty(vm.Groups);
    }

    [Fact]
    public async Task Clear_ConfirmsBeforeClearing()
    {
        var (vm, history, dialogs, _) = Create();

        history.Entries.Add(Entry());

        dialogs.ConfirmResult = false;

        await vm.RefreshCommand.ExecuteAsync(null);
        await vm.ClearCommand.ExecuteAsync(null);

        Assert.Equal(1, dialogs.ConfirmCalls);
        Assert.Single(history.Entries);
        Assert.Single(vm.Groups);
    }

    [Fact]
    public async Task Clear_Confirmed_RemovesAllEntries()
    {
        var (vm, history, dialogs, _) = Create();

        history.Entries.Add(Entry());
        history.Entries.Add(Entry());

        dialogs.ConfirmResult = true;

        await vm.RefreshCommand.ExecuteAsync(null);
        await vm.ClearCommand.ExecuteAsync(null);

        Assert.Equal(1, dialogs.ConfirmCalls);
        Assert.Equal(1, history.ClearCalls);
        Assert.Empty(history.Entries);
        Assert.Empty(vm.Groups);
        Assert.True(vm.IsEmpty);
    }

    [Fact]
    public async Task ExportToPath_WritesCamelCaseJson()
    {
        var (vm, history, _, _) = Create();

        history.Entries.Add(Entry());

        string file = Path.Combine(
            Path.GetTempPath(),
            "WinGetXAvalonia.Tests",
            Guid.NewGuid().ToString("N"),
            "export.json");

        await vm.ExportToPathAsync(file);

        string json = await File.ReadAllTextAsync(file);
        using var doc = JsonDocument.Parse(json);

        Assert.Equal(JsonValueKind.Array, doc.RootElement.ValueKind);
        Assert.Equal(1, doc.RootElement.GetArrayLength());
        Assert.True(doc.RootElement[0].TryGetProperty("packageId", out _));
        Assert.False(doc.RootElement[0].TryGetProperty("PackageId", out _));
    }

    [Theory]
    [InlineData(0, "Today")]
    [InlineData(-1, "Yesterday")]
    [InlineData(-2, "Last 7 Days")]
    [InlineData(-6, "Last 7 Days")]
    [InlineData(-7, "Older")]
    [InlineData(-30, "Older")]
    public void GroupTitle_ClassifiesDates(int daysAgo, string expected)
    {
        var now = new DateTime(2026, 8, 4, 12, 0, 0);

        Assert.Equal(expected, HistoryViewModel.GroupTitle(now.AddDays(daysAgo), now));
    }

    [Fact]
    public void HistoryItem_TitleMapsOperationAndStatus()
    {
        var installed = new HistoryItemViewModel(new HistoryEntry { Operation = "Install", Success = true });
        var updated = new HistoryItemViewModel(new HistoryEntry { Operation = "Update", Success = true });
        var removed = new HistoryItemViewModel(new HistoryEntry { Operation = "Remove", Success = true });
        var failed = new HistoryItemViewModel(new HistoryEntry
        {
            Operation = "Install",
            Success = false,
            Message = "Exit code 1603"
        });

        Assert.Equal("Installed", installed.Title);
        Assert.Equal("Updated", updated.Title);
        Assert.Equal("Removed", removed.Title);
        Assert.Equal("Install Failed", failed.Title);

        Assert.Equal("✓", installed.Icon);
        Assert.Equal("✕", failed.Icon);
        Assert.Equal("Success", installed.Status);
        Assert.Equal("Exit code 1603", failed.Status);
    }
}
