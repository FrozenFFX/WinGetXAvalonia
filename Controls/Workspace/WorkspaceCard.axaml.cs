// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using Avalonia.Controls;
using Avalonia.Interactivity;

namespace WinGetXAvalonia.Controls.Workspace;

public partial class WorkspaceCard : UserControl
{
    public WorkspaceCard()
    {
        InitializeComponent();
    }

    private void ShowContextMenu(object? sender, RoutedEventArgs e)
    {
        ContextMenu?.Open(this);
    }
}
