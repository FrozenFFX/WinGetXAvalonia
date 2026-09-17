// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using WinGetXAvalonia.Services.Notifications;

namespace WinGetXAvalonia.Converters;

public sealed class NotificationTypeToIconConverter : IValueConverter
{
    private static readonly IconGeometryConverter GeometryConverter = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var key = value switch
        {
            NotificationType.Success => "success",
            NotificationType.Information => "info",
            NotificationType.Warning => "warning",
            NotificationType.Error => "error",
            _ => "info"
        };

        return GeometryConverter.Convert(key, typeof(Geometry), null, culture);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
