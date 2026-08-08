// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace WinGetXAvalonia.Controls.Shared;

public partial class AppDialog : ContentControl
{
    private Button? _closeButton;

    public AppDialog()
    {
        InitializeComponent();
    }

    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<AppDialog, bool>(nameof(IsOpen));

    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<AppDialog, string?>(nameof(Title));

    public static readonly StyledProperty<object?> FooterContentProperty =
        AvaloniaProperty.Register<AppDialog, object?>(nameof(FooterContent));

    public static readonly StyledProperty<bool> ShowCloseButtonProperty =
        AvaloniaProperty.Register<AppDialog, bool>(nameof(ShowCloseButton), true);

    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public object? FooterContent
    {
        get => GetValue(FooterContentProperty);
        set => SetValue(FooterContentProperty, value);
    }

    public bool ShowCloseButton
    {
        get => GetValue(ShowCloseButtonProperty);
        set => SetValue(ShowCloseButtonProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_closeButton is not null)
            _closeButton.Click -= OnCloseClicked;

        _closeButton = e.NameScope.Find<Button>("PART_Close");

        if (_closeButton is not null)
            _closeButton.Click += OnCloseClicked;
    }

    private void OnCloseClicked(object? sender, RoutedEventArgs e)
        => IsOpen = false;
}
