// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Globalization;
using Avalonia.Media;
using WinGetXAvalonia.Converters;

namespace WinGetXAvalonia.Tests;

public class IconGeometryConverterTests
{
    private static readonly IconGeometryConverter Converter = new();

    [Fact]
    public void Convert_ValidKey_ReturnsStreamGeometry()
    {
        var result = Converter.Convert("package", typeof(Geometry), null, CultureInfo.InvariantCulture);

        Assert.IsType<StreamGeometry>(result);
    }

    [Fact]
    public void Convert_IconPrefixedKey_ReturnsStreamGeometry()
    {
        var result = Converter.Convert("IconPackage", typeof(Geometry), null, CultureInfo.InvariantCulture);

        Assert.IsType<StreamGeometry>(result);
    }

    [Fact]
    public void Convert_UnknownKey_ReturnsFallbackGeometry()
    {
        Assert.NotNull(Converter.Convert("totallyMissingIcon", typeof(Geometry), null, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void Convert_NullOrWhitespace_ReturnsFallbackGeometry()
    {
        Assert.NotNull(Converter.Convert(null, typeof(Geometry), null, CultureInfo.InvariantCulture));
        Assert.NotNull(Converter.Convert("", typeof(Geometry), null, CultureInfo.InvariantCulture));
        Assert.NotNull(Converter.Convert("   ", typeof(Geometry), null, CultureInfo.InvariantCulture));
    }
}