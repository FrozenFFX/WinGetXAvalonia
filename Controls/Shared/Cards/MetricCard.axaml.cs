// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using Avalonia;
using Avalonia.Controls;

namespace WinGetXAvalonia.Controls.Shared;

public partial class MetricCard : UserControl
{
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<MetricCard, string?>(nameof(Title));

    public static readonly StyledProperty<string?> ValueProperty =
        AvaloniaProperty.Register<MetricCard, string?>(nameof(Value));

    public static readonly StyledProperty<string?> UnitProperty =
        AvaloniaProperty.Register<MetricCard, string?>(nameof(Unit));

    public static readonly StyledProperty<string?> DeltaProperty =
        AvaloniaProperty.Register<MetricCard, string?>(nameof(Delta));

    public static readonly StyledProperty<bool> IsPositiveProperty =
        AvaloniaProperty.Register<MetricCard, bool>(nameof(IsPositive));

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

    public string? Unit
    {
        get => GetValue(UnitProperty);
        set => SetValue(UnitProperty, value);
    }

    public string? Delta
    {
        get => GetValue(DeltaProperty);
        set => SetValue(DeltaProperty, value);
    }

    public bool IsPositive
    {
        get => GetValue(IsPositiveProperty);
        set => SetValue(IsPositiveProperty, value);
    }

    public MetricCard()
    {
        InitializeComponent();
    }
}
