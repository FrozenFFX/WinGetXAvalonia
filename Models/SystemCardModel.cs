using CommunityToolkit.Mvvm.ComponentModel;

namespace WinGetXAvalonia.Models;

public partial class SystemCardModel : ObservableObject
{
    [ObservableProperty]
    private string title = "";

    [ObservableProperty]
    private string value = "";

    [ObservableProperty]
    private string icon = "";
}
