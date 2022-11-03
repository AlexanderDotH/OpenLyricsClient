using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using OpenLyricsClient.Backend.Utils;
using Brush = Avalonia.Media.Brush;
using FontFamily = Avalonia.Media.FontFamily;
using FontStyle = Avalonia.Media.FontStyle;

namespace OpenLyricsClient.Frontend.Models.Elements;

public class LyricsCard : TemplatedControl
{
    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<LyricsCard, string>(nameof(Text));

    public static readonly StyledProperty<double> PercentageProperty =
        AvaloniaProperty.Register<LyricsCard, double>(nameof(Percentage));
    
    public static readonly StyledProperty<Brush> SelectedLineBrushProperty =
        AvaloniaProperty.Register<UserControl, Brush>(nameof(SelectedLineBrush));

    public static readonly StyledProperty<Brush> UnSelectedLineBrushProperty =
        AvaloniaProperty.Register<UserControl, Brush>(nameof(UnSelectedLineBrush));
    
    public static readonly StyledProperty<FontWeight> FontWeightProperty =
        AvaloniaProperty.Register<LyricsCard, FontWeight>(nameof(FontWeight));
    
    public static readonly StyledProperty<int> FontSizeProperty =
        AvaloniaProperty.Register<LyricsCard, int>(nameof(FontSize));
    
    public static readonly StyledProperty<int> SpacingProperty =
        AvaloniaProperty.Register<LyricsCard, int>(nameof(Spacing));
    
    private TextBlock _presenterBlock;
    private TextBlock _greyBlock;
    private Border _border;

    public LyricsCard()
    {

    }
    
    public Rect GetBounds()
    {
        FormattedText text = new FormattedText(Text,
            new Typeface(FontFamily.Parse("avares://Material.Styles/Fonts/Roboto#Roboto"), FontStyle.Normal, FontWeight <= 0 ? FontWeight.Bold : FontWeight), FontSize, TextAlignment.Left,
            TextWrapping.Wrap, this.DesiredSize);
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

                if (this.FontWeight == 0)
                    return;
                
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
    
    public FontWeight FontWeight
    {
        get { return GetValue(FontWeightProperty); }
        set { SetValue(FontWeightProperty, value); }
    }
    
    public int FontSize
    {
        get { return GetValue(FontSizeProperty); }
        set { SetValue(FontSizeProperty, value); }
    }
    
    public int Spacing
    {
        get { return GetValue(SpacingProperty); }
        set { SetValue(SpacingProperty, value); }
    }
    
    public Brush SelectedLineBrush
    {
        get { return GetValue(SelectedLineBrushProperty); }
        set { SetValue(SelectedLineBrushProperty, value); }
    }
    
    public Brush UnSelectedLineBrush
    {
        get { return GetValue(UnSelectedLineBrushProperty); }
        set { SetValue(UnSelectedLineBrushProperty, value); }
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