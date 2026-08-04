using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace WinGetXAvalonia.Models;

public partial class WorkspaceModel : ObservableObject
{
    [ObservableProperty]
    private Guid id = Guid.NewGuid();

    [ObservableProperty]
    private string name = "";

    [ObservableProperty]
    private string description = "";

    [ObservableProperty]
    private string icon = "💻";

    [ObservableProperty]
    private string color = "#4F8CFF";

    [ObservableProperty]
    private DateTime created = DateTime.Now;

    [ObservableProperty]
    private DateTime lastUsed = DateTime.Now;

    [ObservableProperty]
    private ObservableCollection<WorkspacePackage> packages = new();

    public int PackageCount => Packages.Count;
}
