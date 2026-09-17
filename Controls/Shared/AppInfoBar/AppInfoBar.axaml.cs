// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using System.Globalization;
using WinGetXAvalonia.Converters;

namespace WinGetXAvalonia.Controls.Shared;

public partial class AppInfoBar : UserControl
{
    public AppInfoBar()
    {
        InitializeComponent();
        PART_Close.Click += OnCloseClicked;
        UpdateSeverity();
    }

    public static readonly StyledProperty<string?> SeverityProperty =
        AvaloniaProperty.Register<AppInfoBar, string?>(nameof(Severity), "Info");

    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<AppInfoBar, string?>(nameof(Title));

    public static readonly StyledProperty<string?> MessageProperty =
        AvaloniaProperty.Register<AppInfoBar, string?>(nameof(Message));

    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<AppInfoBar, bool>(nameof(IsOpen), true);

    public string? Severity
    {
        get => GetValue(SeverityProperty);
        set => SetValue(SeverityProperty, value);
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

    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SeverityProperty)
            UpdateSeverity();
    }

    private void OnCloseClicked(object? sender, RoutedEventArgs e)
        => IsOpen = false;

    private static readonly IconGeometryConverter IconConverter = new();

    private void UpdateSeverity()
    {
        Classes.Set("info", string.Equals(Severity, "Info", System.StringComparison.OrdinalIgnoreCase));
        Classes.Set("success", string.Equals(Severity, "Success", System.StringComparison.OrdinalIgnoreCase));
        Classes.Set("warning", string.Equals(Severity, "Warning", System.StringComparison.OrdinalIgnoreCase));
        Classes.Set("error", string.Equals(Severity, "Error", System.StringComparison.OrdinalIgnoreCase));

        var (key, brush) = Severity switch
        {
            "Success" => ("success", Avalonia.Application.Current?.Resources["SuccessBrush"] as IBrush),
            "Warning" => ("warning", Avalonia.Application.Current?.Resources["WarningBrush"] as IBrush),
            "Error" => ("error", Avalonia.Application.Current?.Resources["DangerBrush"] as IBrush),
            _ => ("info", Avalonia.Application.Current?.Resources["InfoBrush"] as IBrush)
        };

        PART_Icon.Data = IconConverter.Convert(key, typeof(Geometry), null, CultureInfo.InvariantCulture) as Geometry;
        PART_Icon.Fill = brush;
    }
}
