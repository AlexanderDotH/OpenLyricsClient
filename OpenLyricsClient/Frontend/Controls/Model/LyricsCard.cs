using System;
using System.Drawing;
using System.Globalization;
using System.Linq.Expressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using FontFamily = Avalonia.Media.FontFamily;
using FontStyle = Avalonia.Media.FontStyle;
using Size = Avalonia.Size;

namespace OpenLyricsClient.Frontend.Controls.Model;

public class LyricsCard : TemplatedControl
{
    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<LyricsCard, string>(nameof(Text));

    public static readonly StyledProperty<double> PercentageProperty =
        AvaloniaProperty.Register<LyricsCard, double>(nameof(Percentage));
   
    public Rect GetBounds()
    {
        FormattedText text = new FormattedText(Text,
            new Typeface(FontFamily.Parse("avares://Material.Styles/Fonts/Roboto#Roboto"), FontStyle.Normal, FontWeight.Bold), 30, TextAlignment.Left,
            TextWrapping.Wrap, Size.Empty);
        return text.Bounds;
    }

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public double Percentage
    {
        get => GetValue(PercentageProperty);
        set
        {
            if (value < 0)
            {
                SetValue(PercentageProperty, value);
            }
            else
            {
                SetValue(PercentageProperty, Math.Round(((GetBounds().Width) / 100) * value) + 12);
            }
        }
    }
}