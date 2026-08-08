// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using Avalonia;
using Avalonia.Controls;

namespace WinGetXAvalonia.Controls.Shared;

public partial class AppBadge : UserControl
{
    public AppBadge()
    {
        InitializeComponent();
    }

    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<AppBadge, string?>(nameof(Text));

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
}
