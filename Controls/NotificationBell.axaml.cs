using Avalonia.Controls;
using Avalonia.Interactivity;
using WinGetXAvalonia.ViewModels;

namespace WinGetXAvalonia.Controls;

public partial class NotificationBell : UserControl
{
    public NotificationBell()
    {
        InitializeComponent();

        BellButton.IsCheckedChanged += OnBellToggled;
    }

    private void OnBellToggled(object? sender, RoutedEventArgs e)
    {
        if (BellButton.IsChecked == true && DataContext is NotificationCenterViewModel vm)
            vm.MarkAllReadCommand.Execute(null);
    }
}
