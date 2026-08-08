// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using Avalonia;
using Avalonia.Controls;

namespace WinGetXAvalonia.Controls.Shared;

public partial class AppProgress : UserControl
{
    public AppProgress()
    {
        InitializeComponent();
    }

    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<AppProgress, string?>(nameof(Title));

    public static readonly StyledProperty<double> ValueProperty =
        AvaloniaProperty.Register<AppProgress, double>(nameof(Value), 0);

    public static readonly StyledProperty<bool> IsIndeterminateProperty =
        AvaloniaProperty.Register<AppProgress, bool>(nameof(IsIndeterminate), false);

    public static readonly StyledProperty<bool> ShowValueProperty =
        AvaloniaProperty.Register<AppProgress, bool>(nameof(ShowValue), true);

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public double Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public bool IsIndeterminate
    {
        get => GetValue(IsIndeterminateProperty);
        set => SetValue(IsIndeterminateProperty, value);
    }

    public bool ShowValue
    {
        get => GetValue(ShowValueProperty);
        set => SetValue(ShowValueProperty, value);
    }
}
