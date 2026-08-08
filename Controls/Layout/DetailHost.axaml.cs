// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using Avalonia;
using Avalonia.Controls;

namespace WinGetXAvalonia.Controls.Layout;

public partial class DetailHost : ContentControl
{
    public DetailHost()
    {
        InitializeComponent();
    }

    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<DetailHost, string?>(nameof(Title));

    public static readonly StyledProperty<string?> SubtitleProperty =
        AvaloniaProperty.Register<DetailHost, string?>(nameof(Subtitle));

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string? Subtitle
    {
        get => GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }
}
