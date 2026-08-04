using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace WinGetXAvalonia.Controls.Studio;

public partial class StudioHeader : UserControl
{
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<StudioHeader, string?>(nameof(Title));

    public static readonly StyledProperty<string?> SubtitleProperty =
        AvaloniaProperty.Register<StudioHeader, string?>(nameof(Subtitle));

    public static readonly StyledProperty<string?> DescriptionProperty =
        AvaloniaProperty.Register<StudioHeader, string?>(nameof(Description));

    public static readonly StyledProperty<string?> BadgeProperty =
        AvaloniaProperty.Register<StudioHeader, string?>(nameof(Badge));

    public static readonly StyledProperty<string?> IconProperty =
        AvaloniaProperty.Register<StudioHeader, string?>(nameof(Icon));

    public static readonly StyledProperty<IBrush?> AccentColorProperty =
        AvaloniaProperty.Register<StudioHeader, IBrush?>(nameof(AccentColor));

    public static readonly StyledProperty<string?> PrimaryTextProperty =
        AvaloniaProperty.Register<StudioHeader, string?>(nameof(PrimaryText));

    public static readonly StyledProperty<string?> SecondaryTextProperty =
        AvaloniaProperty.Register<StudioHeader, string?>(nameof(SecondaryText));

    public static readonly StyledProperty<ICommand?> CommandProperty =
        AvaloniaProperty.Register<StudioHeader, ICommand?>(nameof(Command));

    public static readonly StyledProperty<ICommand?> SecondaryCommandProperty =
        AvaloniaProperty.Register<StudioHeader, ICommand?>(nameof(SecondaryCommand));

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string? Subtitle
    {
        get => GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    public string? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public string? Badge
    {
        get => GetValue(BadgeProperty);
        set => SetValue(BadgeProperty, value);
    }

    public string? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public IBrush? AccentColor
    {
        get => GetValue(AccentColorProperty);
        set => SetValue(AccentColorProperty, value);
    }

    public string? PrimaryText
    {
        get => GetValue(PrimaryTextProperty);
        set => SetValue(PrimaryTextProperty, value);
    }

    public string? SecondaryText
    {
        get => GetValue(SecondaryTextProperty);
        set => SetValue(SecondaryTextProperty, value);
    }

    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public ICommand? SecondaryCommand
    {
        get => GetValue(SecondaryCommandProperty);
        set => SetValue(SecondaryCommandProperty, value);
    }

    public StudioHeader()
    {
        InitializeComponent();
    }
}
