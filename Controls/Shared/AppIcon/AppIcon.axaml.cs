// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace WinGetXAvalonia.Controls.Shared;

public partial class AppIcon : UserControl
{
    public AppIcon()
    {
        InitializeComponent();
    }

    public static readonly StyledProperty<string?> SourceProperty =
        AvaloniaProperty.Register<AppIcon, string?>(nameof(Source));

    public static readonly StyledProperty<double> SizeProperty =
        AvaloniaProperty.Register<AppIcon, double>(nameof(Size), 18);

    public static readonly StyledProperty<IBrush?> AccentBrushProperty =
        AvaloniaProperty.Register<AppIcon, IBrush?>(nameof(AccentBrush));

    public string? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public double Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public IBrush? AccentBrush
    {
        get => GetValue(AccentBrushProperty);
        set => SetValue(AccentBrushProperty, value);
    }
}
