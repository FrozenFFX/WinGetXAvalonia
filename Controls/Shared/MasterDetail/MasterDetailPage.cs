// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;

namespace WinGetXAvalonia.Controls.Shared;

public partial class MasterDetailPage : ContentControl
{
    private const double CompactBreakpoint = 760;

    private Grid? _layoutRoot;
    private ContentPresenter? _masterPresenter;
    private ContentPresenter? _detailPresenter;
    private GridSplitter? _detailSplitter;
    private bool _isCompact;

    internal bool IsCompactLayout => _isCompact;

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
        SizeChanged += OnSizeChanged;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _layoutRoot = e.NameScope.Find<Grid>("PART_LayoutRoot");
        _masterPresenter = e.NameScope.Find<ContentPresenter>("PART_Master");
        _detailPresenter = e.NameScope.Find<ContentPresenter>("PART_Detail");
        _detailSplitter = e.NameScope.Find<GridSplitter>("DetailSplitter");
        UpdateResponsiveLayout(Bounds.Width);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == DetailWidthProperty && !_isCompact && _layoutRoot is not null)
            _layoutRoot.ColumnDefinitions[2].Width = DetailWidth;
    }

    private void OnSizeChanged(object? sender, SizeChangedEventArgs e)
        => UpdateResponsiveLayout(e.NewSize.Width);

    private void UpdateResponsiveLayout(double width)
    {
        if (_layoutRoot is null || _masterPresenter is null || _detailPresenter is null)
            return;

        var isCompact = width < CompactBreakpoint;
        if (_isCompact == isCompact && width > 0)
            return;

        _isCompact = isCompact;
        var columns = _layoutRoot.ColumnDefinitions;
        if (_isCompact)
        {
            columns[0].Width = new GridLength(1, GridUnitType.Star);
            columns[0].MinWidth = 0;
            columns[1].Width = new GridLength(0);
            columns[2].Width = new GridLength(0);
            Grid.SetRow(_masterPresenter, 0);
            Grid.SetColumn(_masterPresenter, 0);
            Grid.SetColumnSpan(_masterPresenter, 3);
            Grid.SetRow(_detailPresenter, 1);
            Grid.SetColumn(_detailPresenter, 0);
            Grid.SetColumnSpan(_detailPresenter, 3);
            if (_detailSplitter is not null)
                _detailSplitter.IsVisible = false;
        }
        else
        {
            columns[0].Width = new GridLength(1, GridUnitType.Star);
            columns[0].MinWidth = 300;
            columns[1].Width = new GridLength(2);
            columns[2].Width = DetailWidth;
            Grid.SetRow(_masterPresenter, 0);
            Grid.SetColumn(_masterPresenter, 0);
            Grid.SetColumnSpan(_masterPresenter, 1);
            Grid.SetRow(_detailPresenter, 0);
            Grid.SetColumn(_detailPresenter, 2);
            Grid.SetColumnSpan(_detailPresenter, 1);
            if (_detailSplitter is not null)
                _detailSplitter.IsVisible = true;
        }
    }
}
