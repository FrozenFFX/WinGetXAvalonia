// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Styling;

namespace WinGetXAvalonia.Converters;

public sealed class IconGeometryConverter : IValueConverter
{
    private const string FallbackKey = "package";

    private static readonly Geometry FallbackGeometry = CreateFallbackGeometry();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var source = value as string;

        var app = Application.Current;

        if (app is not null)
        {
            if (!string.IsNullOrWhiteSpace(source))
            {
                var candidate = source.StartsWith("Icon", StringComparison.Ordinal)
                    ? source
                    : "Icon" + char.ToUpperInvariant(source[0]) + source[1..];

                if (app.Resources.TryGetResource(candidate, ThemeVariant.Default, out var resource)
                    && resource is Geometry geometry)
                {
                    return geometry;
                }
            }

            if (app.Resources.TryGetResource("IconPackage", ThemeVariant.Default, out var fallback)
                && fallback is Geometry package)
            {
                return package;
            }
        }

        return FallbackGeometry;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();

    private static Geometry CreateFallbackGeometry()
    {
        var geometry = new StreamGeometry();

        using (var context = geometry.Open())
        {
            context.BeginFigure(new Point(2, 2), isFilled: true);
            context.LineTo(new Point(22, 2));
            context.LineTo(new Point(22, 22));
            context.LineTo(new Point(2, 22));
            context.EndFigure(isClosed: true);
        }

        return geometry;
    }
}
