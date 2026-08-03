// ==========================================================
// WinGetX - https://github.com/FrozenFFX
// ==========================================================
using System.Collections.ObjectModel;

namespace WinGetXAvalonia.ViewModels;

// kaneki
public partial class FavoritesViewModel : ViewModelBase
{
    public ObservableCollection<string> Favorites { get; } = new()
    {
        "Google Chrome",
        "Discord",
        "Visual Studio Code",
        "Spotify"
    };
}
// kaneki
