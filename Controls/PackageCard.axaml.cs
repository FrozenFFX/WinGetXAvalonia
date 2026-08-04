using Avalonia;
using Avalonia.Controls;

namespace WinGetXAvalonia.Controls;

public partial class PackageCard : UserControl
{
    public static readonly StyledProperty<PackageCardMode> ModeProperty =
        AvaloniaProperty.Register<PackageCard, PackageCardMode>(nameof(Mode));

    public PackageCardMode Mode
    {
        get => GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    public PackageCard()
    {
        InitializeComponent();
    }
}
