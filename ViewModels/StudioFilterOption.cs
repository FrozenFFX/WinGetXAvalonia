using CommunityToolkit.Mvvm.ComponentModel;
using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.ViewModels;

public partial class StudioFilterOption : ObservableObject
{
    public StudioToolStatus? Mode { get; init; }

    public string Label { get; init; } = "";

    [ObservableProperty]
    private bool isActive;
}
