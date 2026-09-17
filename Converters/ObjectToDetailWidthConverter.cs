// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace WinGetXAvalonia.Converters;

public sealed class ObjectToDetailWidthConverter : IValueConverter
{
    private static readonly GridLength s_collapsed = new(0);
    private static readonly GridLength s_expanded = new(0.5, GridUnitType.Star);

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is null ? s_collapsed : s_expanded;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
