using Avalonia;
using Avalonia.Controls;

namespace WinGetXAvalonia.Controls.Shared;

public partial class Section : ContentControl
{
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<Section, string?>(nameof(Title));

    public static readonly StyledProperty<string?> DescriptionProperty =
        AvaloniaProperty.Register<Section, string?>(nameof(Description));

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public Section()
    {
        InitializeComponent();
    }
}
