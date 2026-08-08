// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;

namespace WinGetXAvalonia.Tests;

[Collection("UiDispatcher")]
public class FontRenderProbeTests
{
    private static int CountVisiblePixels(Control control)
    {
        control.Measure(Size.Infinity);
        control.Arrange(new Rect(control.DesiredSize));

        var pixelSize = new PixelSize(
            (int)Math.Ceiling(control.Bounds.Width),
            (int)Math.Ceiling(control.Bounds.Height));

        using var bitmap = new RenderTargetBitmap(pixelSize);

        bitmap.Render(control);

        var samples = new byte[pixelSize.Width * pixelSize.Height * 4];

        var pinned = System.Runtime.InteropServices.GCHandle.Alloc(samples, System.Runtime.InteropServices.GCHandleType.Pinned);

        try
        {
            bitmap.CopyPixels(
                new PixelRect(0, 0, pixelSize.Width, pixelSize.Height),
                pinned.AddrOfPinnedObject(),
                samples.Length,
                pixelSize.Width * 4);
        }
        finally
        {
            pinned.Free();
        }

        var visible = 0;

        for (int i = 0; i < samples.Length; i += 4)
        {
            var b = samples[i];
            var g = samples[i + 1];
            var r = samples[i + 2];
            var a = samples[i + 3];

            if (a > 0 && (r > 8 || g > 8 || b > 8))
                visible++;
        }

        return visible;
    }

    private static int CountGlyphPixels(string text, string fontFamily)
    {
        return Dispatcher.UIThread.Invoke(() =>
        {
            var control = new TextBlock
            {
                Text = text,
                FontSize = 36,
                FontFamily = new FontFamily(fontFamily),
                Foreground = Brushes.White,
                Background = Brushes.Black
            };

            return CountVisiblePixels(control);
        });
    }

    [Theory]
    [InlineData("Segoe UI Variable", "12345")]
    [InlineData("Segoe UI", "12345")]
    [InlineData("Segoe UI Variable", "Installed")]
    [InlineData("Segoe UI", "Installed")]
    public void TextBlock_Renders_Glyphs(string fontFamily, string text)
    {
        var visible = CountGlyphPixels(text, fontFamily);

        Assert.True(visible > 100,
            $"Font '{fontFamily}' text '{text}' rendered {visible} visible pixels (expected > 100).");
    }

    [Theory]
    [InlineData("Segoe UI Variable", "📦")]
    [InlineData("Segoe UI", "📦")]
    public void Emoji_TextBlock_Renders_Glyphs(string fontFamily, string text)
    {
        var visible = CountGlyphPixels(text, fontFamily);

        Assert.True(visible > 20,
            $"Font '{fontFamily}' emoji '{text}' rendered {visible} visible pixels (expected > 20).");
    }
}
