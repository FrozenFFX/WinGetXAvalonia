// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace WinGetXAvalonia.Controls;

public partial class PackageCard : UserControl
{
    public static readonly StyledProperty<PackageCardMode> ModeProperty =
        AvaloniaProperty.Register<PackageCard, PackageCardMode>(nameof(Mode));

    public PackageCardMode Mode
    {
        get => GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    public PackageCard()
    {
        InitializeComponent();
    }

    private void ShowContextMenu(object? sender, RoutedEventArgs e)
    {
        if (sender is Control anchor)
            ContextMenu?.Open(anchor);
    }
}
