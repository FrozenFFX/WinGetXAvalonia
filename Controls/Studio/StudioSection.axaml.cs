using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;

namespace WinGetXAvalonia.Controls.Studio;

public partial class StudioSection : UserControl
{
    public static readonly StyledProperty<string?> HeaderIconProperty =
        AvaloniaProperty.Register<StudioSection, string?>(nameof(HeaderIcon));

    public string? HeaderIcon
    {
        get => GetValue(HeaderIconProperty);
        set => SetValue(HeaderIconProperty, value);
    }

    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<StudioSection, string?>(nameof(Title));

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly StyledProperty<string?> DescriptionProperty =
        AvaloniaProperty.Register<StudioSection, string?>(nameof(Description));

    public string? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public static readonly StyledProperty<string?> ActionTextProperty =
        AvaloniaProperty.Register<StudioSection, string?>(nameof(ActionText));

    public string? ActionText
    {
        get => GetValue(ActionTextProperty);
        set => SetValue(ActionTextProperty, value);
    }

    public static readonly StyledProperty<ICommand?> ActionCommandProperty =
        AvaloniaProperty.Register<StudioSection, ICommand?>(nameof(ActionCommand));

    public ICommand? ActionCommand
    {
        get => GetValue(ActionCommandProperty);
        set => SetValue(ActionCommandProperty, value);
    }

    public StudioSection()
    {
        InitializeComponent();
    }
}
