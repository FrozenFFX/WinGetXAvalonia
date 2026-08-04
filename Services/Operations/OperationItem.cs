// ==========================================================
// WinGetX - https://github.com/FrozenFFX
// ==========================================================

using CommunityToolkit.Mvvm.ComponentModel;
using WinGetXAvalonia.Core.Events;

namespace WinGetXAvalonia.Services.Operations;

// kaneki
public sealed partial class OperationItem : ObservableObject
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string PackageId { get; init; } = "";

    public string DisplayName { get; init; } = "";

    public PackageOperation Operation { get; init; }

    [ObservableProperty]
    private OperationStatus status;

    [ObservableProperty]
    private int progress;

    [ObservableProperty]
    private string? message;

    public bool ShowProgress
        => Status is OperationStatus.Pending or OperationStatus.Running;
}
// kaneki
