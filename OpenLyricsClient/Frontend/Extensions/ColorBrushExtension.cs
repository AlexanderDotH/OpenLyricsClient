using System;
using Avalonia.Media;

namespace OpenLyricsClient.Frontend.Extensions;

public static class ColorBrushExtension
{
    public static SolidColorBrush AdjustBrightness(this SolidColorBrush solidColorBrush, double percentage)
    {
        Color color = solidColorBrush.Color;

        byte r = (byte)((color.R * 0.01) * percentage);
        byte g = (byte)((color.G * 0.01) * percentage);
        byte b = (byte)((color.B * 0.01) * percentage);

        r = Math.Clamp(r, (byte)0, (byte)255);
        g = Math.Clamp(g, (byte)0, (byte)255);
        b = Math.Clamp(b, (byte)0, (byte)255);
        
        return new SolidColorBrush(new Color(color.A, r, g, b));
    }
}