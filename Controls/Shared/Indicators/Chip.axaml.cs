// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using Avalonia;
using Avalonia.Controls;
using System.Windows.Input;

namespace WinGetXAvalonia.Controls.Shared;

public partial class Chip : ContentControl
{
    public static readonly StyledProperty<bool> ShowCloseProperty =
        AvaloniaProperty.Register<Chip, bool>(nameof(ShowClose));

    public static readonly StyledProperty<ICommand?> CloseCommandProperty =
        AvaloniaProperty.Register<Chip, ICommand?>(nameof(CloseCommand));

    public bool ShowClose
    {
        get => GetValue(ShowCloseProperty);
        set => SetValue(ShowCloseProperty, value);
    }

    public ICommand? CloseCommand
    {
        get => GetValue(CloseCommandProperty);
        set => SetValue(CloseCommandProperty, value);
    }

    public Chip()
    {
        InitializeComponent();
    }
}
