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
using OpenLyricsClient.Backend.Utils;
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
    
    private TextBlock _presenterBlock;
    private TextBlock _greyBlock;
    private Border _border;

    public Rect GetBounds()
    {
        FormattedText text = new FormattedText(Text,
            new Typeface(FontFamily.Parse("avares://Material.Styles/Fonts/Roboto#Roboto"), FontStyle.Normal, FontWeight.Bold), 30, TextAlignment.Left,
            TextWrapping.Wrap, Size.Empty);
        return text.Bounds;
    }
    
    public Rect GetBounds(string text)
    {
        FormattedText formattedText = new FormattedText(text,
            new Typeface(FontFamily.Parse("avares://Material.Styles/Fonts/Roboto#Roboto"), FontStyle.Normal, FontWeight.Bold), 30, TextAlignment.Left,
            TextWrapping.Wrap, Size.Empty);
        return formattedText.Bounds;
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

                if (DataValidator.ValidateData(this._presenterBlock, this._greyBlock, this._border))
                {
                    if (DataValidator.ValidateData(this._presenterBlock.TextLayout, this._greyBlock.TextLayout))
                    {
                        if (this._presenterBlock.TextLayout.Size.Height < this._greyBlock.TextLayout.Size.Height)
                        {
                            this._presenterBlock.MaxWidth = this._greyBlock.TextLayout.Size.Width;
                            this._border.MaxWidth = Math.Round(((this._presenterBlock.MaxWidth) / 100) * 100);
                        }
                    }
                }
            }
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        var textBlock = e.NameScope.Find("PART_TextBlock");
        var textBlock1 = e.NameScope.Find("PART_TextBlock1");
        var border = e.NameScope.Find("PART_BackgroundBorder");
        
        if (textBlock is TextBlock)
        {
            TextBlock block = (TextBlock)textBlock;
            this._presenterBlock = block;
        }

        if (textBlock1 is TextBlock)
        {
            TextBlock block = (TextBlock)textBlock1;
            this._greyBlock = block;
        }

        if (border is Border)
        {
            Border b = (Border)border;
            this._border = b;
        }
        
        base.OnApplyTemplate(e);
    }
}