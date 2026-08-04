using Avalonia;
using Avalonia.Controls;

namespace WinGetXAvalonia.Controls.Shared;

public enum SidePosition
{
    Left,
    Right
}

public partial class SplitView : UserControl
{
    public static readonly StyledProperty<object?> MainContentProperty =
        AvaloniaProperty.Register<SplitView, object?>(nameof(MainContent));

    public static readonly StyledProperty<object?> SideContentProperty =
        AvaloniaProperty.Register<SplitView, object?>(nameof(SideContent));

    public static readonly StyledProperty<double> SideWidthProperty =
        AvaloniaProperty.Register<SplitView, double>(nameof(SideWidth), 360d);

    public static readonly StyledProperty<SidePosition> SidePositionProperty =
        AvaloniaProperty.Register<SplitView, SidePosition>(nameof(SidePosition), SidePosition.Right);

    public object? MainContent
    {
        get => GetValue(MainContentProperty);
        set => SetValue(MainContentProperty, value);
    }

    public object? SideContent
    {
        get => GetValue(SideContentProperty);
        set => SetValue(SideContentProperty, value);
    }

    public double SideWidth
    {
        get => GetValue(SideWidthProperty);
        set => SetValue(SideWidthProperty, value);
    }

    public SidePosition SidePosition
    {
        get => GetValue(SidePositionProperty);
        set => SetValue(SidePositionProperty, value);
    }

    public SplitView()
    {
        InitializeComponent();
        ApplyLayout();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SideWidthProperty || change.Property == SidePositionProperty)
            ApplyLayout();
    }

    private void ApplyLayout()
    {
        var side = new GridLength(SideWidth, GridUnitType.Pixel);
        var main = new GridLength(1, GridUnitType.Star);

        bool left = SidePosition == SidePosition.Left;

        LayoutGrid.ColumnDefinitions[0].Width = left ? side : main;
        LayoutGrid.ColumnDefinitions[1].Width = left ? main : side;

        MainPresenter.SetValue(Grid.ColumnProperty, left ? 1 : 0);
        SidePresenter.SetValue(Grid.ColumnProperty, left ? 0 : 1);
    }
}
