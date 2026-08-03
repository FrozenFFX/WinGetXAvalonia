// ==========================================================
// WinGetX - https://github.com/FrozenFFX
// ==========================================================
namespace WinGetXAvalonia.ViewModels;

// kaneki
public abstract class PlaceholderViewModel : ViewModelBase
{
    public string Title { get; }

    public string Description { get; }

    protected PlaceholderViewModel(string title, string description)
    {
        Title = title;
        Description = description;
    }
}
// kaneki
