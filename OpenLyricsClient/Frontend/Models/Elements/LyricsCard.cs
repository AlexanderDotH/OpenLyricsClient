using System;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using OpenLyricsClient.Backend;
using OpenLyricsClient.Backend.Structure.Lyrics;
using OpenLyricsClient.Backend.Utils;
using OpenLyricsClient.Frontend.View.Custom;
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
    
    public static readonly DirectProperty<LyricsCard, LyricPart> LyricPartProperty = 
        AvaloniaProperty.RegisterDirect<LyricsCard, LyricPart>(nameof(LyricPart), o => o.LyricPart, (o, v) => o.LyricPart = v);

    public static readonly DirectProperty<LyricsCard, bool> CurrentProperty = 
        AvaloniaProperty.RegisterDirect<LyricsCard, bool>(nameof(Current), o => o.Current, (o, v) => o.Current = v);
    
    private TextBlock _presenterBlock;
    private TextBlock _greyBlock;
    private Border _border;

    private LyricPart _lyricPart;
    private bool _current;
    private double _oldValue;
    
    public LyricsCard()
    {
        this._oldValue = 0;
        
        Core.INSTANCE.LyricHandler.LyricChanged += (sender, args) =>
        {
            if (DataValidator.ValidateData(this._lyricPart))
            {
                if (args.LyricPart.Time != this._lyricPart.Time &&
                    args.LyricPart.Part != this._lyricPart.Part &&
                    args.LyricPart != this._lyricPart)
                {
                    Current = false;
                    Percentage = -10;
                }
                else
                {
                    Current = true;
                }
            }            
        };
    }
    
    public Rect GetBounds()
    {
        if (this.FontSize <= 0)
            return new Rect();
        
        if (this.FontWeight <= 0)
            return new Rect();
        
        FormattedText text = new FormattedText(Text,
            new Typeface(FontFamily.Parse(
                    "avares://Material.Styles/Fonts/Roboto#Roboto, Noto Sans, BlinkMacSystemFont, Segoe UI, Helvetica Neue, Helvetica, Cantarell, Ubuntu, Arial, Hiragino Kaku Gothic Pro, MS UI Gothic, MS PMincho, Microsoft JhengHei, Microsoft JhengHei UI, Microsoft YaHei New, Microsoft Yahei, SimHei"), 
                FontStyle.Normal, this.FontWeight), this.FontSize, TextAlignment.Left,
            TextWrapping.Wrap, new Size(this.Parent.Bounds.Width, 0));

        Rect rect = new Rect(new Size(text.Bounds.Width, Math.Floor(text.Bounds.Height)));
        return rect;
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
                if (this._oldValue == value)
                    return;
                
                this._oldValue = value;
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
    
    public bool Current
    {
        get { return this._current; }
        set
        {
            SetAndRaise(CurrentProperty, ref _current, value);
        }
    }
    
    public LyricPart LyricPart
    {
        get { return this._lyricPart; }
        set
        {
            SetAndRaise(LyricPartProperty, ref _lyricPart, value);
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

    protected override void OnDataContextBeginUpdate()
    {
        /*if (Percentage < 0 || !Current)
        {
            if (DataValidator.ValidateData(this._border))
            {
                this._border.IsVisible = false;
            }

            if (DataValidator.ValidateData(this._presenterBlock))
            {
                this._presenterBlock.IsVisible = false;
            }
        }
        else
        {
            if (DataValidator.ValidateData(this._border))
            {
                this._border.IsVisible = true;
            }

            if (DataValidator.ValidateData(this._presenterBlock))
            {
                this._presenterBlock.IsVisible = true;
            }
        }*/
        
        if (!Current)
        {
            //Percentage = int.MinValue;
        }
        base.OnDataContextBeginUpdate();
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