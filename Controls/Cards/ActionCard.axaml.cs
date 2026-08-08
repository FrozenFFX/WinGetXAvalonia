// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;

namespace WinGetXAvalonia.Controls.Cards;

public partial class ActionCard : UserControl
{
    public ActionCard()
    {
        InitializeComponent();
    }

    public static readonly StyledProperty<string?> IconProperty =
        AvaloniaProperty.Register<ActionCard, string?>(nameof(Icon), string.Empty);

    public string Icon
    {
        get => GetValue(IconProperty) ?? string.Empty;
        set => SetValue(IconProperty, value);
    }

    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<ActionCard, string?>(nameof(Title), string.Empty);

    public string Title
    {
        get => GetValue(TitleProperty) ?? string.Empty;
        set => SetValue(TitleProperty, value);
    }

    public static readonly StyledProperty<string?> DescriptionProperty =
        AvaloniaProperty.Register<ActionCard, string?>(nameof(Description), string.Empty);

    public string Description
    {
        get => GetValue(DescriptionProperty) ?? string.Empty;
        set => SetValue(DescriptionProperty, value);
    }

    public static readonly StyledProperty<ICommand?> CommandProperty =
        AvaloniaProperty.Register<ActionCard, ICommand?>(nameof(Command));

    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }
}
