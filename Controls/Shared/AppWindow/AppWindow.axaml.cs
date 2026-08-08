// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace WinGetXAvalonia.Controls.Shared;

public partial class AppWindow : ContentControl
{
    private Button? _minimizeButton;
    private Button? _maximizeButton;
    private Button? _closeButton;

    public AppWindow()
    {
        InitializeComponent();
    }

    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<AppWindow, string?>(nameof(Title));

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public event EventHandler? MinimizeRequested;

    public event EventHandler? MaximizeRequested;

    public event EventHandler? CloseRequested;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        Unhook();

        _minimizeButton = e.NameScope.Find<Button>("PART_Minimize");
        _maximizeButton = e.NameScope.Find<Button>("PART_Maximize");
        _closeButton = e.NameScope.Find<Button>("PART_Close");

        if (_minimizeButton is not null)
            _minimizeButton.Click += OnMinimize;

        if (_maximizeButton is not null)
            _maximizeButton.Click += OnMaximize;

        if (_closeButton is not null)
            _closeButton.Click += OnClose;
    }

    private void Unhook()
    {
        if (_minimizeButton is not null)
            _minimizeButton.Click -= OnMinimize;

        if (_maximizeButton is not null)
            _maximizeButton.Click -= OnMaximize;

        if (_closeButton is not null)
            _closeButton.Click -= OnClose;
    }

    private void OnMinimize(object? sender, RoutedEventArgs e)
    {
        MinimizeRequested?.Invoke(this, EventArgs.Empty);

        if (VisualRoot is Window window)
            window.WindowState = WindowState.Minimized;
    }

    private void OnMaximize(object? sender, RoutedEventArgs e)
    {
        MaximizeRequested?.Invoke(this, EventArgs.Empty);

        if (VisualRoot is Window window)
            window.WindowState = window.WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
    }

    private void OnClose(object? sender, RoutedEventArgs e)
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);

        if (VisualRoot is Window window)
            window.Close();
    }
}
