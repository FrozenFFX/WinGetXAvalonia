// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using Avalonia.Controls;
using Avalonia;
using System.ComponentModel;
using WinGetXAvalonia.ViewModels;

namespace WinGetXAvalonia.Views;

public partial class DashboardView : UserControl
{
    private bool _isCompact;
    private DashboardViewModel? _viewModel;

    internal bool IsCompactLayout => _isCompact;

    public DashboardView()
    {
        InitializeComponent();
        SizeChanged += OnSizeChanged;
        DataContextChanged += OnDataContextChanged;
        UpdateResponsiveLayout(Bounds.Width);
    }

    private void OnSizeChanged(object? sender, SizeChangedEventArgs e)
        => UpdateResponsiveLayout(e.NewSize.Width);

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (_viewModel is not null)
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;

        _viewModel = DataContext as DashboardViewModel;
        if (_viewModel is not null)
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;

        UpdateResponsiveLayout(Bounds.Width);
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DashboardViewModel.SystemPanelVisible))
            UpdateSystemPanelWidth();
    }

    private void UpdateResponsiveLayout(double width)
    {
        var isCompact = width < 760;
        if (_isCompact == isCompact && width > 0)
        {
            UpdateSystemPanelWidth();
            return;
        }

        _isCompact = isCompact;
        if (_isCompact)
        {
            Grid.SetRow(SystemPanel, 1);
            Grid.SetColumn(SystemPanel, 0);
            Grid.SetColumnSpan(SystemPanel, 3);
            DashboardContent.ColumnDefinitions[1].Width = new GridLength(0);
            DashboardContent.ColumnDefinitions[2].Width = new GridLength(0);
            SystemPanelSeparator.IsVisible = false;
        }
        else
        {
            Grid.SetRow(SystemPanel, 0);
            Grid.SetColumn(SystemPanel, 2);
            Grid.SetColumnSpan(SystemPanel, 1);
            DashboardContent.ColumnDefinitions[1].Width = new GridLength(16);
            SystemPanelSeparator.IsVisible = true;
            UpdateSystemPanelWidth();
        }
    }

    private void UpdateSystemPanelWidth()
    {
        if (!_isCompact)
        {
            var isVisible = _viewModel?.SystemPanelVisible == true;
            DashboardContent.ColumnDefinitions[2].Width =
                isVisible ? new GridLength(340) : new GridLength(0);
        }
    }
}
