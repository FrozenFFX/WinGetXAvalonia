// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using Avalonia.Controls;
using Avalonia.Input;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.ViewModels;

namespace WinGetXAvalonia.Controls.Layout;

public partial class LeftSidebar : UserControl
{
    public LeftSidebar()
    {
        InitializeComponent();
    }

    // Kategori başlığı tıklaması ListBox seçimine karışmaz. Seçim yalnızca
    // gerçek sayfa öğelerinde değişir; başlık yalnızca grubu açar/kapar
    // (accordion). e.Handled, başlığın seçilmesini engeller.
    private void OnHeaderPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Control { DataContext: NavigationItem { IsCategoryHeader: true } header })
            return;

        e.Handled = true;

        if (DataContext is MainViewModel vm)
            vm.ToggleGroupCommand.Execute(header);
    }
}
