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
        if (DataContext is not BrowseViewModel vm)
            return;

        if (e.Key == Key.Enter)
        {
            if (vm.Suggestions.Count > 0)
                vm.SearchText = vm.Suggestions[0];

            vm.SearchCommand.Execute(null);
            e.Handled = true;
        }
        else if (e.Key == Key.Down && vm.Suggestions.Count > 0)
        {
            vm.SearchText = vm.Suggestions[0];
            e.Handled = true;
        }
    }

    private void OnSearchSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0 && DataContext is BrowseViewModel vm)
            vm.SearchCommand.Execute(null);
    }
}
// kaneki
