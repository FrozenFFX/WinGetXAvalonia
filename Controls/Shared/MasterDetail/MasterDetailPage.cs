// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace WinGetXAvalonia.Controls.Shared;

public partial class MasterDetailPage : ContentControl
{
    public static readonly StyledProperty<object?> MasterContentProperty =
        AvaloniaProperty.Register<MasterDetailPage, object?>(nameof(MasterContent));

    public static readonly StyledProperty<IDataTemplate?> MasterContentTemplateProperty =
        AvaloniaProperty.Register<MasterDetailPage, IDataTemplate?>(nameof(MasterContentTemplate));

    public static readonly StyledProperty<object?> DetailContentProperty =
        AvaloniaProperty.Register<MasterDetailPage, object?>(nameof(DetailContent));

    public static readonly StyledProperty<IDataTemplate?> DetailContentTemplateProperty =
        AvaloniaProperty.Register<MasterDetailPage, IDataTemplate?>(nameof(DetailContentTemplate));

    public static readonly StyledProperty<bool> IsLoadingProperty =
        AvaloniaProperty.Register<MasterDetailPage, bool>(nameof(IsLoading));

    public static readonly StyledProperty<string?> LoadingMessageProperty =
        AvaloniaProperty.Register<MasterDetailPage, string?>(nameof(LoadingMessage));

    public static readonly StyledProperty<GridLength> DetailWidthProperty =
        AvaloniaProperty.Register<MasterDetailPage, GridLength>(nameof(DetailWidth), new GridLength(360));

    public object? MasterContent
    {
        get => GetValue(MasterContentProperty);
        set => SetValue(MasterContentProperty, value);
    }

    public IDataTemplate? MasterContentTemplate
    {
        get => GetValue(MasterContentTemplateProperty);
        set => SetValue(MasterContentTemplateProperty, value);
    }

    public object? DetailContent
    {
        get => GetValue(DetailContentProperty);
        set => SetValue(DetailContentProperty, value);
    }

    public IDataTemplate? DetailContentTemplate
    {
        get => GetValue(DetailContentTemplateProperty);
        set => SetValue(DetailContentTemplateProperty, value);
    }

    public bool IsLoading
    {
        get => GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public string? LoadingMessage
    {
        get => GetValue(LoadingMessageProperty);
        set => SetValue(LoadingMessageProperty, value);
    }

    public GridLength DetailWidth
    {
        get => GetValue(DetailWidthProperty);
        set => SetValue(DetailWidthProperty, value);
    }

    public MasterDetailPage()
    {
        InitializeComponent();
    }
}
