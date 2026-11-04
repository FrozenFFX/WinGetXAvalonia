// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using Avalonia;
using Avalonia.Controls;

namespace WinGetXAvalonia.Views;

public partial class BrowseView : UserControl
{
    private bool _isCompact;

    internal bool IsCompactLayout => _isCompact;

    public BrowseView()
    {
        InitializeComponent();
        SizeChanged += OnSizeChanged;
        UpdateResponsiveLayout(Bounds.Width);
    }

    private void OnSizeChanged(object? sender, SizeChangedEventArgs e)
        => UpdateResponsiveLayout(e.NewSize.Width);

    private void UpdateResponsiveLayout(double width)
    {
        var isCompact = width < 760;
        if (_isCompact == isCompact && width > 0)
            return;

        _isCompact = isCompact;
        var columns = BrowseLayout.ColumnDefinitions;
        if (_isCompact)
        {
            columns[0].Width = new GridLength(1, GridUnitType.Star);
            columns[1].Width = new GridLength(0);
            columns[2].Width = new GridLength(0);
            Grid.SetRow(BrowseDetails, 1);
            Grid.SetColumn(BrowseDetails, 0);
            Grid.SetColumnSpan(BrowseDetails, 3);
            BrowseDetails.Margin = new Thickness(0, 12, 0, 0);
            BrowseDetailSplitter.IsVisible = false;
            BrowseLayout.Margin = new Thickness(4);
        }
        else
        {
            columns[0].Width = new GridLength(65, GridUnitType.Star);
            columns[1].Width = new GridLength(12);
            columns[2].Width = new GridLength(35, GridUnitType.Star);
            Grid.SetRow(BrowseDetails, 0);
            Grid.SetColumn(BrowseDetails, 2);
            Grid.SetColumnSpan(BrowseDetails, 1);
            BrowseDetails.Margin = new Thickness(16, 0, 0, 0);
            BrowseDetailSplitter.IsVisible = true;
            BrowseLayout.Margin = new Thickness(24);
        }
    }
}
