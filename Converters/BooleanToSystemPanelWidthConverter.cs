// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace WinGetXAvalonia.Converters;

public sealed class BooleanToSystemPanelWidthConverter : IValueConverter
{
    private static readonly GridLength s_collapsed = new(0);
    private static readonly GridLength s_expanded = new(340);

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? s_expanded : s_collapsed;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
