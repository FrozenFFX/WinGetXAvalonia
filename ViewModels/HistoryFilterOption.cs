using CommunityToolkit.Mvvm.ComponentModel;
using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.ViewModels;

public partial class HistoryFilterOption : ObservableObject
{
    public HistoryFilterMode Mode { get; init; }

    public string Label { get; init; } = "";

    [ObservableProperty]
    private bool isActive;
}
