using System.Globalization;
using Avalonia.Data.Converters;
using WinGetXAvalonia.Controls;

namespace WinGetXAvalonia.Converters;

public sealed class PackageCardModeToBoolConverter : IValueConverter
{
    public PackageCardMode Match { get; set; } = PackageCardMode.Default;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is PackageCardMode mode && mode == Match;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
