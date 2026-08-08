// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

namespace WinGetXAvalonia.Controls.Shared;

public partial class AppDrawer : ContentControl
{
    private Border? _scrim;
    private Border? _panel;

    public AppDrawer()
    {
        InitializeComponent();
    }

    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<AppDrawer, bool>(nameof(IsOpen));

    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<AppDrawer, string?>(nameof(Title));

    public static readonly StyledProperty<double> PanelWidthProperty =
        AvaloniaProperty.Register<AppDrawer, double>(nameof(PanelWidth), 360);

    public static readonly StyledProperty<Dock> DockProperty =
        AvaloniaProperty.Register<AppDrawer, Dock>("Dock", Dock.Left);

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

    public double PanelWidth
    {
        get => GetValue(PanelWidthProperty);
        set => SetValue(PanelWidthProperty, value);
    }

    public Dock Dock
    {
        get => GetValue(DockProperty);
        set => SetValue(DockProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_scrim is not null)
            _scrim.PointerPressed -= OnScrimPressed;

        _scrim = e.NameScope.Find<Border>("PART_Scrim");
        _panel = e.NameScope.Find<Border>("PART_Panel");

        if (_scrim is not null)
            _scrim.PointerPressed += OnScrimPressed;

        ApplyDock();
    }

    private void OnScrimPressed(object? sender, PointerPressedEventArgs e)
        => IsOpen = false;

    private void ApplyDock()
    {
        if (_panel is null)
            return;

        _panel.HorizontalAlignment = Dock == Dock.Right
            ? Avalonia.Layout.HorizontalAlignment.Right
            : Avalonia.Layout.HorizontalAlignment.Left;
    }
}
