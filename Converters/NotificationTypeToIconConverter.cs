using System.Globalization;
using Avalonia.Data.Converters;
using WinGetXAvalonia.Services.Notifications;

namespace WinGetXAvalonia.Converters;

public sealed class NotificationTypeToIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value switch
        {
            NotificationType.Success => "✔",
            NotificationType.Information => "ℹ️",
            NotificationType.Warning => "⚠️",
            NotificationType.Error => "❌",
            _ => "•"
        };

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
