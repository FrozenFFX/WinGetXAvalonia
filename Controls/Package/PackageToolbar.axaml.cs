using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Controls.Package;

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

    public static readonly StyledProperty<string?> ActionTextProperty =
        AvaloniaProperty.Register<PackageToolbar, string?>(nameof(ActionText));

    public static readonly StyledProperty<ICommand?> ActionCommandProperty =
        AvaloniaProperty.Register<PackageToolbar, ICommand?>(nameof(ActionCommand));

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

    public string? ActionText
    {
        get => GetValue(ActionTextProperty);
        set => SetValue(ActionTextProperty, value);
    }

    public ICommand? ActionCommand
    {
        get => GetValue(ActionCommandProperty);
        set => SetValue(ActionCommandProperty, value);
    }

    public ObservableCollection<PackageSortMode> SortItems { get; } =
    [
        PackageSortMode.Name,
        PackageSortMode.Version,
        PackageSortMode.Source,
        PackageSortMode.Available
    ];

    public ObservableCollection<PackageFilterMode> FilterItems { get; } =
    [
        PackageFilterMode.All,
        PackageFilterMode.Winget,
        PackageFilterMode.MsStore,
        PackageFilterMode.Portable
    ];

    public PackageToolbar()
    {
        InitializeComponent();
    }
}
