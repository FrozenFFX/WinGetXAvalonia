// ==========================================================
// WinGetX - https://github.com/FrozenFFX
// ==========================================================
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace WinGetXAvalonia.Controls.Dashboard;

// kaneki
public partial class StatCard : UserControl
{
    public StatCard()
    {
        InitializeComponent();
    }

    public static readonly StyledProperty<string> IconProperty =
        AvaloniaProperty.Register<StatCard, string>(nameof(Icon), string.Empty);

    public string Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<StatCard, string>(nameof(Title), string.Empty);

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly StyledProperty<string> ValueProperty =
        AvaloniaProperty.Register<StatCard, string>(nameof(Value), string.Empty);

    public string Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly StyledProperty<string> SubtitleProperty =
        AvaloniaProperty.Register<StatCard, string>(nameof(Subtitle), string.Empty);

    public string Subtitle
    {
        get => GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    public static readonly StyledProperty<IBrush?> AccentBrushProperty =
        AvaloniaProperty.Register<StatCard, IBrush?>(nameof(AccentBrush));

    public IBrush? AccentBrush
    {
        get => GetValue(AccentBrushProperty);
        set => SetValue(AccentBrushProperty, value);
    }
}
// kaneki
