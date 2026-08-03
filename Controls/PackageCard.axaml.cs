// ==========================================================
// WinGetX - https://github.com/FrozenFFX
// ==========================================================
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Windows.Input;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.ViewModels;

namespace WinGetXAvalonia.Controls;

// kaneki
public partial class PackageCard : UserControl
{
    public PackageCard()
    {
        InitializeComponent();
    }

    private void OnOpenHomepage(object? sender, RoutedEventArgs e)
        => Route(nameof(InstalledViewModel.OpenHomepageCommand));

    private void OnCopyId(object? sender, RoutedEventArgs e)
        => Route(nameof(InstalledViewModel.CopyIdCommand));

    private void OnCopyName(object? sender, RoutedEventArgs e)
        => Route(nameof(InstalledViewModel.CopyNameCommand));

    private void Route(string commandName)
    {
        if (DataContext is not PackageModel package)
            return;

        if (typeof(InstalledViewModel)
            .GetProperty(commandName)?
            .GetValue(FindViewModel()) is ICommand executable
            && executable.CanExecute(package))
        {
            executable.Execute(package);
        }
    }

    private InstalledViewModel? FindViewModel()
    {
        for (StyledElement? parent = Parent; parent is not null; parent = parent.Parent)
        {
            if (parent is ItemsControl { DataContext: InstalledViewModel viewModel })
                return viewModel;
        }

        return null;
    }
}
// kaneki
