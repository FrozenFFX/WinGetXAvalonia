namespace WinGetXAvalonia.Models;

public class NavigationItem
{
    public string Title { get; set; } = "";

    public string Icon { get; set; } = "";

    public Type Page { get; set; } = typeof(object);
}
