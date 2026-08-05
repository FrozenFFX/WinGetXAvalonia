// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinGetXAvalonia.Core.State;
using WinGetXAvalonia.Services.Performance;

namespace WinGetXAvalonia.ViewModels;

public partial class DeveloperViewModel : AsyncViewModel
{
    private readonly IPerformanceService _performance;

    public QueuePanelViewModel Queue { get; }

    public PerformanceSnapshot Snapshot => _performance.Capture();

    [ObservableProperty]
    private bool diagnosticsEnabled = true;

    public DeveloperViewModel(
        QueuePanelViewModel queue,
        IPerformanceService performance)
    {
        Queue = queue;
        _performance = performance;
    }

    public Task InitializeAsync()
        => Refresh();

    [RelayCommand]
    private Task Refresh()
    {
        State = LoadState.Loaded;
        return Task.CompletedTask;
    }

    [RelayCommand]
    private void ClearLogs()
    {
    }

    [RelayCommand]
    private void ExportLogs()
    {
    }
}
