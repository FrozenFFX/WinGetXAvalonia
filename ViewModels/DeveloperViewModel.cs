namespace WinGetXAvalonia.ViewModels;

public partial class DeveloperViewModel : ViewModelBase
{
    public QueuePanelViewModel Queue { get; }

    public DeveloperViewModel(QueuePanelViewModel queue)
    {
        Queue = queue;
    }
}
