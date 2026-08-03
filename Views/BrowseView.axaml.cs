// ==========================================================
// WinGetX - https://github.com/FrozenFFX
// ==========================================================
using Avalonia.Controls;
using Avalonia.Input;
using WinGetXAvalonia.ViewModels;

namespace WinGetXAvalonia.Views;

// kaneki
public partial class BrowseView : UserControl
{
    public BrowseView()
    {
        InitializeComponent();
    }

    private void OnSearchKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && DataContext is BrowseViewModel vm)
            vm.SearchCommand.Execute(null);
    }

    private void OnSearchSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0 && DataContext is BrowseViewModel vm)
            vm.SearchCommand.Execute(null);
    }
}
// kaneki
