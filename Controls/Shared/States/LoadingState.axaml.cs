using Avalonia;
using Avalonia.Controls;

namespace WinGetXAvalonia.Controls.Shared;

public partial class LoadingState : UserControl
{
    public static readonly StyledProperty<string?> MessageProperty =
        AvaloniaProperty.Register<LoadingState, string?>(nameof(Message));

    public string? Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public LoadingState()
    {
        InitializeComponent();
    }
}
