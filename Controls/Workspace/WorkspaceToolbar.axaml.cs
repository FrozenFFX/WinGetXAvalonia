using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;

namespace WinGetXAvalonia.Controls.Workspace;

public partial class WorkspaceToolbar : UserControl
{
    public static readonly StyledProperty<string?> SearchTextProperty =
        AvaloniaProperty.Register<WorkspaceToolbar, string?>(nameof(SearchText),
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    public static readonly StyledProperty<int> CountProperty =
        AvaloniaProperty.Register<WorkspaceToolbar, int>(nameof(Count));

    public static readonly StyledProperty<ICommand?> CreateCommandProperty =
        AvaloniaProperty.Register<WorkspaceToolbar, ICommand?>(nameof(CreateCommand));

    public static readonly StyledProperty<ICommand?> ImportCommandProperty =
        AvaloniaProperty.Register<WorkspaceToolbar, ICommand?>(nameof(ImportCommand));

    public static readonly StyledProperty<ICommand?> RefreshCommandProperty =
        AvaloniaProperty.Register<WorkspaceToolbar, ICommand?>(nameof(RefreshCommand));

    public string? SearchText
    {
        get => GetValue(SearchTextProperty);
        set => SetValue(SearchTextProperty, value);
    }

    public int Count
    {
        get => GetValue(CountProperty);
        set => SetValue(CountProperty, value);
    }

    public ICommand? CreateCommand
    {
        get => GetValue(CreateCommandProperty);
        set => SetValue(CreateCommandProperty, value);
    }

    public ICommand? ImportCommand
    {
        get => GetValue(ImportCommandProperty);
        set => SetValue(ImportCommandProperty, value);
    }

    public ICommand? RefreshCommand
    {
        get => GetValue(RefreshCommandProperty);
        set => SetValue(RefreshCommandProperty, value);
    }

    public WorkspaceToolbar()
    {
        InitializeComponent();
    }
}
