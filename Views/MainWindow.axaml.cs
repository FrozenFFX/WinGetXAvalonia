// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using WinGetXAvalonia.ViewModels;

namespace WinGetXAvalonia.Views;

public partial class MainWindow : Window
{
    private const double CompactBreakpoint = 1024;

    private MainViewModel? _viewModel;
    private bool _isCompact;
    private bool _isMobileMenuOpen;

    public MainWindow()
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
        {
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            _viewModel.OperationDrawer.PropertyChanged -= OnOperationsPropertyChanged;
        }

        _viewModel = DataContext as MainViewModel;
        if (_viewModel is not null)
        {
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
            _viewModel.OperationDrawer.PropertyChanged += OnOperationsPropertyChanged;
        }

        UpdateSidePanelWidths();
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.IsDetailVisible))
            UpdateSidePanelWidths();

        if (_isCompact && e.PropertyName == nameof(MainViewModel.SelectedMenuItem))
            SetMobileMenuOpen(false);
    }

    private void OnOperationsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(OperationDrawerViewModel.HasOperations))
            UpdateSidePanelWidths();
    }

    private void UpdateResponsiveLayout(double width)
    {
        var isCompact = width < CompactBreakpoint;
        if (_isCompact == isCompact && width > 0)
            return;

        _isCompact = isCompact;
        if (!_isCompact)
            _isMobileMenuOpen = false;

        MobileMenuButton.IsVisible = _isCompact;
        MainContentScroller.Margin = _isCompact
            ? new Thickness(12, 14, 12, 12)
            : new Thickness(28, 24, 28, 20);
        Grid.SetColumn(MainContentScroller, _isCompact ? 0 : 1);
        Grid.SetColumnSpan(MainContentScroller, _isCompact ? 4 : 1);

        DesktopSidebar.Width = _isCompact ? 272 : double.NaN;
        Grid.SetColumnSpan(DesktopSidebar, _isCompact ? 2 : 1);
        SetMobileMenuOpen(_isCompact && _isMobileMenuOpen);
        UpdateSidePanelWidths();
    }

    private void UpdateSidePanelWidths()
    {
        if (ShellRoot.ColumnDefinitions.Count < 4)
            return;

        var columns = ShellRoot.ColumnDefinitions;
        columns[0].Width = _isCompact ? new GridLength(0) : new GridLength(232);
        columns[2].Width = !_isCompact && _viewModel?.IsDetailVisible == true
            ? new GridLength(0.5, GridUnitType.Star)
            : new GridLength(0);
        columns[3].Width = !_isCompact && _viewModel?.OperationDrawer.HasOperations == true
            ? new GridLength(0.5, GridUnitType.Star)
            : new GridLength(0);
    }

    private void SetMobileMenuOpen(bool isOpen)
    {
        _isMobileMenuOpen = _isCompact && isOpen;
        DesktopSidebar.IsVisible = !_isCompact || _isMobileMenuOpen;
        MobileScrim.IsVisible = _isCompact && _isMobileMenuOpen;
    }

    private void OnMobileMenuClick(object? sender, RoutedEventArgs e)
        => SetMobileMenuOpen(!_isMobileMenuOpen);

    private void OnMobileScrimPressed(object? sender, PointerPressedEventArgs e)
    {
        SetMobileMenuOpen(false);
        e.Handled = true;
    }
}
