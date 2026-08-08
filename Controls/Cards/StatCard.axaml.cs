// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;

namespace WinGetXAvalonia.Controls.Cards;

public partial class StatCard : UserControl
{
    public StatCard()
    {
        InitializeComponent();
    }

    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<StatCard, string?>(nameof(Title));

    public static readonly StyledProperty<string?> ValueProperty =
        AvaloniaProperty.Register<StatCard, string?>(nameof(Value));

    public static readonly StyledProperty<string?> IconProperty =
        AvaloniaProperty.Register<StatCard, string?>(nameof(Icon));

    public static readonly StyledProperty<string?> SubtitleProperty =
        AvaloniaProperty.Register<StatCard, string?>(nameof(Subtitle));

    public static readonly StyledProperty<string?> SecondaryProperty =
        AvaloniaProperty.Register<StatCard, string?>(nameof(Secondary));

    public static readonly StyledProperty<double> ProgressProperty =
        AvaloniaProperty.Register<StatCard, double>(nameof(Progress), 100);

    public static readonly StyledProperty<ICommand?> CommandProperty =
        AvaloniaProperty.Register<StatCard, ICommand?>(nameof(Command));

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public string? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public string? Subtitle
    {
        get => GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    public string? Secondary
    {
        get => GetValue(SecondaryProperty);
        set => SetValue(SecondaryProperty, value);
    }

    public double Progress
    {
        get => GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }
}
