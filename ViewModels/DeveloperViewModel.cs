// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinGetXAvalonia.Core.State;

namespace WinGetXAvalonia.ViewModels;

public partial class DeveloperViewModel : AsyncViewModel
{
    public QueuePanelViewModel Queue { get; }

    [ObservableProperty]
    private bool diagnosticsEnabled = true;

    public DeveloperViewModel(QueuePanelViewModel queue)
    {
        Queue = queue;
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
