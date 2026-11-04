// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace WinGetXAvalonia.Converters;

/// <summary>
/// Kullanılabilir genişliği kolon sayısına çevirir. Sabit genişlik hack'i
/// yerine dashboard'un responsive kolon davranışını belirler.
/// </summary>
public sealed class ResponsiveColumnsConverter : IValueConverter
{
    public int Wide { get; set; } = 4;

    public int Medium { get; set; } = 2;

    public int Narrow { get; set; } = 1;

    public double WideThreshold { get; set; } = 1180;

    public double MediumThreshold { get; set; } = 760;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var width = value switch
        {
            double d => d,
            Rect rect => rect.Width,
            _ => 0d
        };

        if (width <= 0)
            return Wide;

        if (width >= WideThreshold)
            return Wide;

        return width >= MediumThreshold ? Medium : Narrow;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
