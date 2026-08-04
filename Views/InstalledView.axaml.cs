// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using Avalonia.Controls;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.ViewModels;

namespace WinGetXAvalonia.Views;

public partial class InstalledView : UserControl
{
    public InstalledView()
    {
        InitializeComponent();
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not InstalledViewModel vm)
            return;

        foreach (PackageModel package in e.AddedItems)
            package.Selected = true;

        foreach (PackageModel package in e.RemovedItems)
            package.Selected = false;
    }
}
