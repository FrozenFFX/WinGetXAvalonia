using CommunityToolkit.Mvvm.ComponentModel;

namespace WinGetXAvalonia.Models;

public partial class PackageModel : ObservableObject
{
    [ObservableProperty]
    private string icon = "📦";

    [ObservableProperty]
    private string name = "";

    [ObservableProperty]
    private string id = "";

    [ObservableProperty]
    private string version = "";

    [ObservableProperty]
    private string available = "";

    [ObservableProperty]
    private string source = "";

    [ObservableProperty]
    private bool selected;
}
