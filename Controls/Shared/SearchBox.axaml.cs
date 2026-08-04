// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;

namespace WinGetXAvalonia.Controls.Shared;

public partial class SearchBox : UserControl
{
    public SearchBox()
    {
        InitializeComponent();
    }

    public static readonly StyledProperty<string?> WatermarkProperty =
        AvaloniaProperty.Register<SearchBox, string?>(
            nameof(Watermark));

    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<SearchBox, string?>(
            nameof(Text),
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<object?> LeftIconProperty =
        AvaloniaProperty.Register<SearchBox, object?>(
            nameof(LeftIcon));

    public static readonly StyledProperty<bool> ShowClearButtonProperty =
        AvaloniaProperty.Register<SearchBox, bool>(
            nameof(ShowClearButton),
            true);

    public string? Watermark
    {
        get => GetValue(WatermarkProperty);
        set => SetValue(WatermarkProperty, value);
    }

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public object? LeftIcon
    {
        get => GetValue(LeftIconProperty);
        set => SetValue(LeftIconProperty, value);
    }

    public bool ShowClearButton
    {
        get => GetValue(ShowClearButtonProperty);
        set => SetValue(ShowClearButtonProperty, value);
    }

    private void Clear_Click(object? sender, RoutedEventArgs e)
    {
        Text = string.Empty;
    }
}