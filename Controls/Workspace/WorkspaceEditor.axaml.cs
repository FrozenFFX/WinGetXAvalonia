using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;

namespace WinGetXAvalonia.Controls.Workspace;

public partial class WorkspaceEditor : UserControl
{
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<WorkspaceEditor, string?>(nameof(Title));

    public new static readonly StyledProperty<string?> NameProperty =
        AvaloniaProperty.Register<WorkspaceEditor, string?>(
            nameof(Name),
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    public static readonly StyledProperty<string?> DescriptionProperty =
        AvaloniaProperty.Register<WorkspaceEditor, string?>(
            nameof(Description),
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    public static readonly StyledProperty<ICommand?> SaveCommandProperty =
        AvaloniaProperty.Register<WorkspaceEditor, ICommand?>(nameof(SaveCommand));

    public static readonly StyledProperty<string?> SaveButtonTextProperty =
        AvaloniaProperty.Register<WorkspaceEditor, string?>(nameof(SaveButtonText));

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public new string? Name
    {
        get => GetValue(NameProperty);
        set => SetValue(NameProperty, value);
    }

    public string? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public ICommand? SaveCommand
    {
        get => GetValue(SaveCommandProperty);
        set => SetValue(SaveCommandProperty, value);
    }

    public string? SaveButtonText
    {
        get => GetValue(SaveButtonTextProperty);
        set => SetValue(SaveButtonTextProperty, value);
    }

    public WorkspaceEditor()
    {
        InitializeComponent();
    }
}
