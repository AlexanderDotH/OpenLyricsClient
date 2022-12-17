using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Threading;
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
    private Viewbox _viewbox;
    private Panel _panel;
    private NoteAnimation _noteAnimation;

    private LyricPart _lyricPart;
    private bool _current;
    private double _oldValue;

    private bool _templateApplied;
    private bool _validLyricSet;
    private bool _alreadySet;
    
    public LyricsCard()
    {
        this._oldValue = 0;

        this._templateApplied = false;
        this._validLyricSet = false;
        this._alreadySet = false;
        
        Core.INSTANCE.SongHandler.SongChanged += (s, args) =>
        {
            this._templateApplied = false;
            this._validLyricSet = false;
            this._alreadySet = false;
        };
        
        Core.INSTANCE.LyricHandler.LyricChanged += (sender, args) =>
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                this.InvalidateVisual();
                
                if (DataValidator.ValidateData(this._lyricPart))
                {
                    if (!args.LyricPart.Equals(this._lyricPart))
                    {
                        Current = false;
                        Percentage = -10;
                        this._noteAnimation.Current = false;
                        this._noteAnimation.Percentage = -10;
                    }
                    else
                    {
                        Current = true;
                        this._noteAnimation.Current = true;
                    }
                }

                if (!(DataValidator.ValidateData(this._presenterBlock, this._greyBlock, this._noteAnimation,
                        this._border)))
                    return;

                if (!(DataValidator.ValidateData(this._presenterBlock.Text) &&
                      DataValidator.ValidateData(this._greyBlock.Text)))
                    return;

                if (this._presenterBlock.Text.Equals("♪") || this._greyBlock.Text.Equals("♪"))
                {
                    this._presenterBlock.IsVisible = false;
                    this._greyBlock.IsVisible = false;
                    this._noteAnimation.IsVisible = true;
                    this._border.IsVisible = false;
                }
                else
                {
                    this._presenterBlock.IsVisible = true;
                    this._greyBlock.IsVisible = true;
                    this._noteAnimation.IsVisible = false;
                    this._border.IsVisible = true;
                }
            });
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
                    "avares://Material.Styles/Fonts/Roboto#Roboto"), 
                FontStyle.Normal, this.FontWeight), this.FontSize, TextAlignment.Left,
            TextWrapping.Wrap, new Size(this._greyBlock.DesiredSize.Width, this._greyBlock.DesiredSize.Height));

        Rect rect = new Rect(new Size(text.Bounds.Width, text.Bounds.Height));
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

                this._noteAnimation.Percentage = value;
                this._oldValue = value;

                SetValue(PercentageProperty, value);
            }
        }
    }
    
    public bool Current
    {
        get { return this._current; }
        set
        {
            SetAndRaise(CurrentProperty, ref _current, value);
            // this._alreadySet = true;
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
        set
        {
            SetValue(FontWeightProperty, value); 
        }
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


    public override void Render(DrawingContext context)
    {
        this._presenterBlock.MaxWidth = this._greyBlock.TextLayout.Size.Width;
        this._presenterBlock.Width = this._greyBlock.TextLayout.Size.Width;
        
        this._viewbox.MaxWidth = this._greyBlock.DesiredSize.Width;
        this._viewbox.Width = ((this._viewbox.MaxWidth) / 100) * Percentage;
        this._border.Width = ((this._viewbox.MaxWidth) / 100) * Percentage;

        if (!this._current)
        {
        }

        base.Render(context);
    }

    protected override void OnTemplateChanged(AvaloniaPropertyChangedEventArgs e)
    {
        base.OnTemplateChanged(e);

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
        var viewBox = e.NameScope.Find("PART_Viewbox");
        var noteAnimation = e.NameScope.Find("PART_NoteAnimation");
        var panel = e.NameScope.Find("PART_Panel");

        if (noteAnimation is NoteAnimation)
        {
            NoteAnimation animation = ((NoteAnimation)(noteAnimation));
            this._noteAnimation = animation;
        }
        
        if (panel is Panel)
        {
            Panel p = (Panel)panel;
            this._panel = p;
        }
        
        if (border is Border)
        {
            Border b = (Border)border;
            this._border = b;
        }
        
        if (viewBox is Viewbox)
        {
            Viewbox box = (Viewbox)viewBox;
            this._viewbox = box;
        }
        
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

        
        base.OnApplyTemplate(e);

    }
}