using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace WinGetXAvalonia.Controls.Dashboard;

public partial class QuickActionCard : UserControl
{
    public QuickActionCard()
    {
        InitializeComponent();
    }

    public static readonly StyledProperty<string> IconProperty =
        AvaloniaProperty.Register<QuickActionCard, string>(nameof(Icon), string.Empty);

    public string Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<QuickActionCard, string>(nameof(Title), string.Empty);

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly StyledProperty<string> DescriptionProperty =
        AvaloniaProperty.Register<QuickActionCard, string>(nameof(Description), string.Empty);

    public string Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public static readonly StyledProperty<ICommand?> CommandProperty =
        AvaloniaProperty.Register<QuickActionCard, ICommand?>(nameof(Command));

    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (Command?.CanExecute(null) == true)
            Command.Execute(null);
    }
}
