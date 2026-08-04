using Avalonia;
using Avalonia.Controls;

namespace WinGetXAvalonia.Controls.Shared;

public partial class StatCard : UserControl
{
    public static readonly StyledProperty<string?> IconProperty =
        AvaloniaProperty.Register<StatCard, string?>(nameof(Icon));

    public static readonly StyledProperty<string?> ValueProperty =
        AvaloniaProperty.Register<StatCard, string?>(nameof(Value));

    public static readonly StyledProperty<string?> LabelProperty =
        AvaloniaProperty.Register<StatCard, string?>(nameof(Label));

    public string? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public string? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public string? Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public StatCard()
    {
        InitializeComponent();
    }
}
