// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Converters;

// Durum göstergesi noktasının rengi temaya duyarlı kaynaklardan gelir (madde 19).
public sealed class StatusToBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        string key = value is AppStatus status
            ? status switch
            {
                AppStatus.Ready => "SuccessBrush",
                AppStatus.Working or AppStatus.Loading => "AccentBrush",
                AppStatus.Connecting => "InfoBrush",
                AppStatus.Offline => "NavHoverBrush",
                AppStatus.Error => "DangerBrush",
                _ => "SuccessBrush"
            }
            : "SuccessBrush";

        return Application.Current?.TryFindResource(key, out var resource) == true
            ? resource
            : AvaloniaProperty.UnsetValue;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
