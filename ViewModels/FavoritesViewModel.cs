using System.Collections.ObjectModel;

namespace WinGetXAvalonia.ViewModels;

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
