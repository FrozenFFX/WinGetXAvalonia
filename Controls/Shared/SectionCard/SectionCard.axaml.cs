// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using Avalonia;
using Avalonia.Controls;

namespace WinGetXAvalonia.Controls.Shared;

public partial class SectionCard : ContentControl
{
    public SectionCard()
    {
        InitializeComponent();
    }

    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<SectionCard, string?>(nameof(Title));

    public static readonly StyledProperty<object?> HeaderContentProperty =
        AvaloniaProperty.Register<SectionCard, object?>(nameof(HeaderContent));

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public object? HeaderContent
    {
        get => GetValue(HeaderContentProperty);
        set => SetValue(HeaderContentProperty, value);
    }
}
