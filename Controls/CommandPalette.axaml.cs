// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.ViewModels;

namespace WinGetXAvalonia.Controls;

public partial class CommandPalette : UserControl
{
    public CommandPalette()
    {
        InitializeComponent();

        PropertyChanged += OnAvaloniaPropertyChanged;
    }

    private void OnAvaloniaPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == IsVisibleProperty && e.GetNewValue<bool>())
        {
            SearchBox.Focus();

            if (!string.IsNullOrEmpty(SearchBox.Text))
                SearchBox.SelectAll();
        }
    }

    private void OnBackdropTapped(object? sender, RoutedEventArgs e)
    {
        if (DataContext is CommandPaletteViewModel viewModel)
            viewModel.CloseCommand.Execute(null);
    }

    private void OnListDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is not CommandPaletteViewModel viewModel)
            return;

        if (e.Source is Control { DataContext: CommandPaletteItem item })
            viewModel.ExecuteCommand.Execute(item);
    }
}
