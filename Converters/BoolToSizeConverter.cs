using System.Globalization;
using Avalonia.Data.Converters;

namespace WinGetXAvalonia.Converters;

public sealed class BoolToSizeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter is not string raw || string.IsNullOrWhiteSpace(raw))
            return Avalonia.AvaloniaProperty.UnsetValue;

        var parts = raw.Split(';');

        if (parts.Length < 2)
            return Avalonia.AvaloniaProperty.UnsetValue;

        var selected = parts[0];
        var unselected = parts[1];

        return double.TryParse(value is true ? selected : unselected,
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out var result)
            ? result
            : Avalonia.AvaloniaProperty.UnsetValue;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
