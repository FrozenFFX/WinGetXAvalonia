// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

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

    public static readonly StyledProperty<ICommand?> CancelCommandProperty =
        AvaloniaProperty.Register<WorkspaceEditor, ICommand?>(nameof(CancelCommand));

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

    public ICommand? CancelCommand
    {
        get => GetValue(CancelCommandProperty);
        set => SetValue(CancelCommandProperty, value);
    }

    public WorkspaceEditor()
    {
        InitializeComponent();
    }
}
