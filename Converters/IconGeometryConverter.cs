// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace WinGetXAvalonia.Converters;

public sealed class IconGeometryConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string source || string.IsNullOrWhiteSpace(source))
            return null;

        string key = "Icon" + char.ToUpperInvariant(source[0]) + source[1..];

        var app = Application.Current;

        return app is not null
               && app.Resources.TryGetResource(key, app.ActualThemeVariant, out var resource)
               && resource is Geometry geometry
            ? geometry
            : null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
