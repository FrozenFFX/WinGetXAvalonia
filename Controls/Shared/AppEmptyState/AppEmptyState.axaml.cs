// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Transformation;
using System.Collections.Generic;
using System.Windows.Input;

namespace WinGetXAvalonia.Controls.Shared;

public partial class AppEmptyState : UserControl
{
    private readonly IList<string> _examples = new List<string>();

    public AppEmptyState()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<AppEmptyState, string?>(nameof(Title));

    public static readonly StyledProperty<string?> DescriptionProperty =
        AvaloniaProperty.Register<AppEmptyState, string?>(nameof(Description));

    public static readonly StyledProperty<string?> IconProperty =
        AvaloniaProperty.Register<AppEmptyState, string?>(nameof(Icon));

    public static readonly StyledProperty<string?> ActionTextProperty =
        AvaloniaProperty.Register<AppEmptyState, string?>(nameof(ActionText));

    public static readonly StyledProperty<ICommand?> ActionCommandProperty =
        AvaloniaProperty.Register<AppEmptyState, ICommand?>(nameof(ActionCommand));

    public static readonly StyledProperty<IEnumerable<string>?> ExamplesProperty =
        AvaloniaProperty.Register<AppEmptyState, IEnumerable<string>?>(nameof(Examples));

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

    public string? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public string? ActionText
    {
        get => GetValue(ActionTextProperty);
        set => SetValue(ActionTextProperty, value);
    }

    public ICommand? ActionCommand
    {
        get => GetValue(ActionCommandProperty);
        set => SetValue(ActionCommandProperty, value);
    }

    public IEnumerable<string>? Examples
    {
        get => GetValue(ExamplesProperty) ?? _examples;
        set => SetValue(ExamplesProperty, value);
    }

    private void OnLoaded(object? sender, System.EventArgs e)
    {
        if (RootPanel.Opacity == 1)
            return;

        RootPanel.Opacity = 1;
        RootPanel.RenderTransform = TransformOperations.Parse("translate(0,0)");
    }
}
