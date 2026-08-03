// ==========================================================
// WinGetX - https://github.com/FrozenFFX
// ==========================================================
using System.Globalization;
using Avalonia.Data.Converters;

namespace WinGetXAvalonia.Converters;

// kaneki
public sealed class ZeroToBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is int number ? number == 0 : false;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
// kaneki
