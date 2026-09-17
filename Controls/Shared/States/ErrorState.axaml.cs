// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using Avalonia;
using Avalonia.Controls;
using System.Windows.Input;

namespace WinGetXAvalonia.Controls.Shared;

public partial class ErrorState : UserControl
{
    public static readonly StyledProperty<string?> IconProperty =
        AvaloniaProperty.Register<ErrorState, string?>(nameof(Icon), "warning");

    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<ErrorState, string?>(nameof(Title), "Something went wrong");

    public static readonly StyledProperty<string?> MessageProperty =
        AvaloniaProperty.Register<ErrorState, string?>(nameof(Message));

    public static readonly StyledProperty<string?> RetryTextProperty =
        AvaloniaProperty.Register<ErrorState, string?>(nameof(RetryText), "Retry");

    public static readonly StyledProperty<ICommand?> RetryCommandProperty =
        AvaloniaProperty.Register<ErrorState, ICommand?>(nameof(RetryCommand));

    public string? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string? Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public string? RetryText
    {
        get => GetValue(RetryTextProperty);
        set => SetValue(RetryTextProperty, value);
    }

    public ICommand? RetryCommand
    {
        get => GetValue(RetryCommandProperty);
        set => SetValue(RetryCommandProperty, value);
    }

    public ErrorState()
    {
        InitializeComponent();
    }
}
