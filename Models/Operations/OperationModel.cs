// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace WinGetXAvalonia.Models.Operations;

public partial class OperationModel : ObservableObject
{
    public Guid Id { get; } = Guid.NewGuid();

    [ObservableProperty]
    private string title = "";

    [ObservableProperty]
    private string subtitle = "";

    [ObservableProperty]
    private string statusMessage = "";

    [ObservableProperty]
    private double progress;

    [ObservableProperty]
    private OperationStatus status;

    [ObservableProperty]
    private bool canCancel = true;

    [ObservableProperty]
    private bool canPause = true;

    [ObservableProperty]
    private DateTime startedAt = DateTime.UtcNow;

    [ObservableProperty]
    private DateTime completedAt = DateTime.UtcNow;

    public TimeSpan Duration
    {
        get
        {
            if (CompletedAt < StartedAt)
                return TimeSpan.Zero;

            return CompletedAt - StartedAt;
        }
    }

    public bool CanRetry =>
        Status is OperationStatus.Failed or OperationStatus.Cancelled;

    partial void OnStatusChanged(OperationStatus value)
        => OnPropertyChanged(nameof(CanRetry));

    partial void OnStartedAtChanged(DateTime value)
        => OnPropertyChanged(nameof(Duration));

    partial void OnCompletedAtChanged(DateTime value)
        => OnPropertyChanged(nameof(Duration));

    [RelayCommand]
    private void Retry()
    {
        Status = OperationStatus.Waiting;
        Progress = 0;
        StatusMessage = "";
    }

    [RelayCommand]
    private void Cancel()
    {
        Status = OperationStatus.Cancelled;
        StatusMessage = "Cancelled";
    }
}
