// ==========================================================
// WinGetX - https://github.com/FrozenFFX
// ==========================================================
using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;

namespace WinGetXAvalonia.Controls.Package;

// kaneki
public partial class PackageToolbar : UserControl
{
    public static readonly StyledProperty<string?> SearchTextProperty =
        AvaloniaProperty.Register<PackageToolbar, string?>(nameof(SearchText),
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    public static readonly StyledProperty<int> PackageCountProperty =
        AvaloniaProperty.Register<PackageToolbar, int>(nameof(PackageCount));

    public static readonly StyledProperty<object?> SelectedSortProperty =
        AvaloniaProperty.Register<PackageToolbar, object?>(nameof(SelectedSort),
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    public static readonly StyledProperty<object?> SelectedFilterProperty =
        AvaloniaProperty.Register<PackageToolbar, object?>(nameof(SelectedFilter),
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    public static readonly StyledProperty<ICommand?> RefreshCommandProperty =
        AvaloniaProperty.Register<PackageToolbar, ICommand?>(nameof(RefreshCommand));

    public string? SearchText
    {
        get => GetValue(SearchTextProperty);
        set => SetValue(SearchTextProperty, value);
    }

    public int PackageCount
    {
        get => GetValue(PackageCountProperty);
        set => SetValue(PackageCountProperty, value);
    }

    public object? SelectedSort
    {
        get => GetValue(SelectedSortProperty);
        set => SetValue(SelectedSortProperty, value);
    }

    public object? SelectedFilter
    {
        get => GetValue(SelectedFilterProperty);
        set => SetValue(SelectedFilterProperty, value);
    }

    public ICommand? RefreshCommand
    {
        get => GetValue(RefreshCommandProperty);
        set => SetValue(RefreshCommandProperty, value);
    }

    public ObservableCollection<string> SortItems { get; } =
    [
        "Name",
        "Version",
        "Source",
        "Available"
    ];

    public ObservableCollection<string> FilterItems { get; } =
    [
        "All",
        "Winget",
        "MS Store",
        "Portable"
    ];

    public PackageToolbar()
    {
        InitializeComponent();
    }
}
// kaneki
