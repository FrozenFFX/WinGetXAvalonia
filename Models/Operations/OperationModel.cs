using CommunityToolkit.Mvvm.ComponentModel;

namespace WinGetXAvalonia.Models.Operations;

public partial class OperationModel : ObservableObject
{
    public Guid Id { get; } = Guid.NewGuid();

    [ObservableProperty]
    private string title = "";

    [ObservableProperty]
    private string subtitle = "";

    [ObservableProperty]
    private double progress;

    [ObservableProperty]
    private OperationStatus status;

    [ObservableProperty]
    private bool canCancel = true;

    [ObservableProperty]
    private bool canPause = true;
}
