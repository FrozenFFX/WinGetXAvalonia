// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Backup;
using WinGetXAvalonia.Services.Blueprints;
using WinGetXAvalonia.Services.Dialogs;
using WinGetXAvalonia.ViewModels;

namespace WinGetXAvalonia.Tests;

public class BlueprintsViewModelTests
{
    private sealed class FakeBlueprintStore : IBlueprintStore
    {
        public List<BlueprintModel> Items { get; } = new();

        public List<Guid> Saved { get; } = new();

        public List<Guid> Deleted { get; } = new();

        public Task<IReadOnlyList<BlueprintModel>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<BlueprintModel>>(Items.ToList());

        public Task SaveAsync(BlueprintModel blueprint, CancellationToken cancellationToken = default)
        {
            Saved.Add(blueprint.Id);
            Items.RemoveAll(i => i.Id == blueprint.Id);
            Items.Add(blueprint);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            Deleted.Add(id);
            Items.RemoveAll(i => i.Id == id);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<BlueprintModel>> ImportJsonAsync(string json, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<BlueprintModel>>(new List<BlueprintModel>());
    }

    private sealed class FakeExportService : IExportService
    {
        public Task ExportJsonAsync<T>(IEnumerable<T> items, string file)
            => Task.CompletedTask;
    }

    private sealed class FakeDialogService : IDialogService
    {
        public bool ConfirmationResult { get; set; } = true;

        public Task<bool> ConfirmAsync(string title, string message, string? confirmText = null, string? cancelText = null)
            => Task.FromResult(ConfirmationResult);

        public Task ProgressAsync(string title, Func<IProgress<double>, Task> operation)
            => Task.CompletedTask;

        public Task ShowErrorAsync(string title, string message)
            => Task.CompletedTask;
    }

    private sealed class FakeFilePickerService : IFilePickerService
    {
        public Task<Avalonia.Platform.Storage.IStorageFile?> PickOpenJsonAsync(string title, CancellationToken cancellationToken = default)
            => Task.FromResult<Avalonia.Platform.Storage.IStorageFile?>(null);

        public Task<Avalonia.Platform.Storage.IStorageFile?> PickSaveJsonAsync(string title, string suggestedFileName, CancellationToken cancellationToken = default)
            => Task.FromResult<Avalonia.Platform.Storage.IStorageFile?>(null);

        public Task<Avalonia.Platform.Storage.IStorageFolder?> PickFolderAsync(string title, CancellationToken cancellationToken = default)
            => Task.FromResult<Avalonia.Platform.Storage.IStorageFolder?>(null);
    }

    private static (BlueprintsViewModel Vm, FakeBlueprintStore Store) Create()
    {
        var store = new FakeBlueprintStore();
        var vm = new BlueprintsViewModel(
            new FakeExportService(),
            new FakeDialogService(),
            new FakeFilePickerService(),
            store);

        return (vm, store);
    }

    [Fact]
    public async Task Refresh_LoadsItemsFromStore()
    {
        var (vm, store) = Create();
        store.Items.Add(new BlueprintModel { Name = "Dev Setup" });

        await vm.Refresh();

        Assert.Single(vm.Blueprints);
        Assert.Single(vm.FilteredBlueprints);
        Assert.Equal("Dev Setup", vm.FilteredBlueprints[0].Name);
    }

    [Fact]
    public async Task Save_NewBlueprint_PersistsToStore()
    {
        var (vm, store) = Create();

        vm.Name = "New Blueprint";
        vm.Description = "Description";
        vm.Steps = "1. Step";

        await vm.SaveCommand.ExecuteAsync(null);

        var saved = Assert.Single(store.Saved);
        Assert.Equal(saved, store.Items.Single().Id);
        Assert.NotNull(vm.SelectedBlueprint);
    }

    [Fact]
    public async Task Save_ExistingBlueprint_UpdatesAndPersists()
    {
        var (vm, store) = Create();
        var blueprint = new BlueprintModel { Name = "Existing" };
        store.Items.Add(blueprint);

        await vm.Refresh();
        vm.SelectedBlueprint = blueprint;

        vm.Name = "Renamed";
        await vm.SaveCommand.ExecuteAsync(null);

        Assert.Equal("Renamed", store.Items.Single().Name);
        Assert.Contains(blueprint.Id, store.Saved);
    }

    [Fact]
    public async Task Delete_RemovesFromStore()
    {
        var (vm, store) = Create();
        var blueprint = new BlueprintModel { Name = "To Delete" };
        store.Items.Add(blueprint);

        await vm.Refresh();
        vm.SelectedBlueprint = blueprint;

        await vm.DeleteCommand.ExecuteAsync(null);

        Assert.Contains(blueprint.Id, store.Deleted);
        Assert.Empty(vm.Blueprints);
        Assert.Null(vm.SelectedBlueprint);
    }

    [Fact]
    public async Task Delete_WithoutSelection_DoesNothing()
    {
        var (vm, store) = Create();

        await vm.DeleteCommand.ExecuteAsync(null);

        Assert.Empty(store.Deleted);
    }
}
