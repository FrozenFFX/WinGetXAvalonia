using CommunityToolkit.Mvvm.ComponentModel;
using WinGetXAvalonia.Core.PackageEngine;

namespace WinGetXAvalonia.Services.Queue;

public partial class QueueItem : ObservableObject
{
    public Guid Id { get; } = Guid.NewGuid();

    public PackageRequest Request { get; set; } = new();

    [ObservableProperty]
    private QueueStatus status;

    public DateTime Created { get; } = DateTime.Now;

    [ObservableProperty]
    private DateTime? started;

    [ObservableProperty]
    private DateTime? finished;

    [ObservableProperty]
    private string? error;
}
