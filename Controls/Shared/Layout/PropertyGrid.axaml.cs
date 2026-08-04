using Avalonia;
using Avalonia.Controls;
using System.Collections;

namespace WinGetXAvalonia.Controls.Shared;

public partial class PropertyGrid : UserControl
{
    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        AvaloniaProperty.Register<PropertyGrid, IEnumerable?>(nameof(ItemsSource));

    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public PropertyGrid()
    {
        InitializeComponent();
    }
}
