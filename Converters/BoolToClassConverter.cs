using System.Globalization;
using Avalonia.Data.Converters;

namespace WinGetXAvalonia.Converters;

public sealed class BoolToClassConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? parameter as string ?? "IsTrue" : "";

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
