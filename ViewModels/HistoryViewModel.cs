// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using WinGetXAvalonia.Core.Domain.History;
using WinGetXAvalonia.Core.Events;
using WinGetXAvalonia.Core.State;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Dialogs;
using WinGetXAvalonia.Services.History;

namespace WinGetXAvalonia.ViewModels;

public partial class HistoryViewModel : AsyncViewModel, IDisposable
{
    private static readonly JsonSerializerOptions ExportOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly IHistoryService _history;
    private readonly IDialogService _dialogs;
    private readonly IEventBus _eventBus;

    private readonly List<HistoryItemViewModel> _allItems = new();
    private IDisposable? _subscription;

    public ObservableCollection<HistoryFilterOption> Filters { get; } = new();

    public ObservableCollection<HistoryGroupViewModel> Groups { get; } = new();

    [ObservableProperty]
    private string searchText = "";

    [ObservableProperty]
    private HistoryFilterMode selectedFilter = HistoryFilterMode.All;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OperationsSummary))]
    private int operationsCount;

    public string OperationsSummary => OperationsCount switch
    {
        0 => "No operations yet",
        1 => "1 operation",
        var count => $"{count} operations"
    };

    public int TotalOperations => _allItems.Count;

    public int SuccessfulOperations => _allItems.Count(i => i.IsSuccess);

    public int FailedOperations => _allItems.Count(i => !i.IsSuccess);

    public int TodayOperations => _allItems.Count(i => i.Entry.Timestamp.Date == DateTime.Today);

    public HistoryViewModel(IHistoryService history, IDialogService dialogs, IEventBus eventBus)
    {
        _history = history;
        _dialogs = dialogs;
        _eventBus = eventBus;

        _subscription =
            _eventBus.Subscribe<PackageChangedEvent>(e =>
                Dispatcher.UIThread.Post(() => _ = RefreshAsync()));

        SeedFilters();

        Groups.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(TotalOperations));
        };
    }

    public override Task InitializeAsync()
        => RefreshAsync();

    partial void OnSearchTextChanged(string value)
        => ApplyFilter();

    partial void OnSelectedFilterChanged(HistoryFilterMode value)
    {
        foreach (var filter in Filters)
            filter.IsActive = filter.Mode == value;

        ApplyFilter();
    }

    [RelayCommand]
    private void SelectFilter(HistoryFilterOption? option)
    {
        if (option is not null)
            SelectedFilter = option.Mode;
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        State = LoadState.Loading;
        ErrorMessage = null;

        try
        {
            var entries = await _history.GetAllAsync();

            _allItems.Clear();

            foreach (var entry in entries.OrderByDescending(e => e.Timestamp))
                _allItems.Add(new HistoryItemViewModel(entry));

            OperationsCount = _allItems.Count;

            OnPropertyChanged(nameof(TotalOperations));
            OnPropertyChanged(nameof(SuccessfulOperations));
            OnPropertyChanged(nameof(FailedOperations));
            OnPropertyChanged(nameof(TodayOperations));

            ApplyFilter();

            State = _allItems.Count == 0 ? LoadState.Empty : LoadState.Loaded;
        }
        catch (Exception ex)
        {
            State = LoadState.Error;
            ErrorMessage = ex.Message;
        }
    }

    [RelayCommand]
    private async Task Clear()
    {
        bool confirmed = await _dialogs.ConfirmAsync(
            "Clear History",
            "Are you sure?\n\nThis action cannot be undone.",
            "Clear");

        if (!confirmed)
            return;

        await _history.ClearAsync();

        await RefreshAsync();

        Groups.Clear();

        OperationsCount = 0;
    }

    [RelayCommand]
    private async Task Export()
    {
        var file = await PickSaveFileAsync();

        if (file is null)
            return;

        try
        {
            await ExportToPathAsync(file.Path.LocalPath);
        }
        catch (Exception ex)
        {
            await _dialogs.ShowErrorAsync("Export Failed", ex.Message);
        }
    }

    public async Task ExportToPathAsync(string path)
    {
        var entries = await _history.GetAllAsync();

        string json =
            JsonSerializer.Serialize(
                entries.OrderByDescending(x => x.Timestamp),
                ExportOptions);

        string? directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        await File.WriteAllTextAsync(path, json);
    }

    private void ApplyFilter()
    {
        IEnumerable<HistoryItemViewModel> filtered = _allItems;

        filtered = SelectedFilter switch
        {
            HistoryFilterMode.Installed => filtered.Where(i => i.IsOperation("Install")),
            HistoryFilterMode.Updated => filtered.Where(i => i.IsOperation("Update")),
            HistoryFilterMode.Removed => filtered.Where(i => i.IsOperation("Remove")),
            HistoryFilterMode.Failed => filtered.Where(i => !i.IsSuccess),
            _ => filtered
        };

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            filtered = filtered.Where(i =>
                i.Entry.PackageName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                || i.Entry.PackageId.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                || i.Entry.Operation.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
        }

        Groups.Clear();

        foreach (var group in BuildGroups(filtered.ToList(), DateTime.Now))
            Groups.Add(group);

        OnPropertyChanged(nameof(TotalOperations));
    }

    public static List<HistoryGroupViewModel> BuildGroups(
        IReadOnlyList<HistoryItemViewModel> items,
        DateTime now)
    {
        var groups = new List<HistoryGroupViewModel>();

        foreach (var group in HistoryGrouper.Group(items.Select(i => i.Entry), now))
        {
            groups.Add(new HistoryGroupViewModel
            {
                Title = group.Title,
                Items = group.Entries.Select(e => new HistoryItemViewModel(e)).ToList()
            });
        }

        return groups;
    }

    public static string GroupTitle(DateTime timestamp, DateTime now)
        => HistoryGrouper.GroupTitle(timestamp, now);

    private static async Task<IStorageFile?> PickSaveFileAsync()
    {
        var window = CurrentWindow;

        if (window is null)
            return null;

        return await window.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Export History",
            SuggestedFileName = $"history-{DateTime.Now:yyyyMMdd-HHmmss}.json",
            DefaultExtension = "json",
            FileTypeChoices = new[]
            {
                new FilePickerFileType("JSON file") { Patterns = new[] { "*.json" } }
            }
        });
    }

    private static Avalonia.Controls.Window? CurrentWindow
        => Avalonia.Application.Current?.ApplicationLifetime
            is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime { MainWindow: { } window }
            ? window
            : null;

    private void SeedFilters()
    {
        Filters.Add(new() { Mode = HistoryFilterMode.All, Label = "All", IsActive = true });
        Filters.Add(new() { Mode = HistoryFilterMode.Installed, Label = "Installed" });
        Filters.Add(new() { Mode = HistoryFilterMode.Updated, Label = "Updated" });
        Filters.Add(new() { Mode = HistoryFilterMode.Removed, Label = "Removed" });
        Filters.Add(new() { Mode = HistoryFilterMode.Failed, Label = "Failed" });
    }

    public void Dispose()
    {
        _subscription?.Dispose();
    }
}

