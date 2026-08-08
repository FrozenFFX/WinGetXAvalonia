// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using Avalonia;
using Avalonia.Controls;

namespace WinGetXAvalonia.Controls.Shared;

public partial class AppSection : ContentControl
{
    public AppSection()
    {
        InitializeComponent();
    }

    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<AppSection, string?>(nameof(Title));

    public static readonly StyledProperty<string?> DescriptionProperty =
        AvaloniaProperty.Register<AppSection, string?>(nameof(Description));

    public static readonly StyledProperty<object?> HeaderContentProperty =
        AvaloniaProperty.Register<AppSection, object?>(nameof(HeaderContent));

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public object? HeaderContent
    {
        get => GetValue(HeaderContentProperty);
        set => SetValue(HeaderContentProperty, value);
    }
}
