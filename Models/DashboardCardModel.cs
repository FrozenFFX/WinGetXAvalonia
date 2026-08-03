// ==========================================================
// WinGetX - https://github.com/FrozenFFX
// ==========================================================
using CommunityToolkit.Mvvm.ComponentModel;

namespace WinGetXAvalonia.Models;

// kaneki
public partial class DashboardCardModel : ObservableObject
{
    [ObservableProperty]
    private string title = "";

    [ObservableProperty]
    private string value = "";

    [ObservableProperty]
    private string icon = "";
}
// kaneki
