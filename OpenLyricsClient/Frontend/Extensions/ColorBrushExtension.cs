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

    public static SolidColorBrush Merge(this SolidColorBrush from, SolidColorBrush to, double percentage)
    {
        Color color = from.Color;
        Color otherColor = to.Color;

        double p = Math.Clamp(percentage, 0, 100);
            
        double red = (otherColor.R * (1 - p) + color.R * p);
        double green = (otherColor.G * (1 - p) + color.G * p);
        double blue = (otherColor.B * (1 - p) + color.B * p);

        Color newColor = new Color(
            255,
            (byte)Math.Clamp(red, 0, 255), 
            (byte)Math.Clamp(green, 0, 255), 
            (byte)Math.Clamp(blue, 0, 255));

        from.Color = newColor;
        return from;
    }
}