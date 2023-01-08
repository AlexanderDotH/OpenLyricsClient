using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using OpenLyricsClient.Backend.Utils;

namespace OpenLyricsClient.Frontend.Models.Elements;

public class NoteAnimation : TemplatedControl
{
    public static readonly StyledProperty<double> PercentageProperty =
        AvaloniaProperty.Register<NoteAnimation, double>(nameof(Percentage));
    
    public static readonly StyledProperty<Brush> SelectedLineBrushProperty =
        AvaloniaProperty.Register<NoteAnimation, Brush>(nameof(SelectedLineBrush));

    public static readonly StyledProperty<Brush> UnSelectedLineBrushProperty =
        AvaloniaProperty.Register<NoteAnimation, Brush>(nameof(UnSelectedLineBrush));

    public static readonly DirectProperty<LyricsCard, bool> CurrentProperty = 
        AvaloniaProperty.RegisterDirect<LyricsCard, bool>(nameof(Current), o => o.Current, (o, v) => o.Current = v);

    private Viewbox _viewbox;
    private Border _border;
    
    private bool _current;
    
    public NoteAnimation()
    {
        FontSize = 30;
        FontWeight = FontWeight.Bold;
        this._current = false;
    }
    
    public Rect GetBounds(string textToMeasure)
    {
        if (this.FontSize <= 0)
            return new Rect();
        
        if (this.FontWeight <= 0)
            return new Rect();

        FormattedText text = new FormattedText(textToMeasure,
            new Typeface(FontFamily.Parse(
                    "avares://Material.Styles/Fonts/Roboto#Roboto"), 
                FontStyle.Normal, this.FontWeight), this.FontSize, TextAlignment.Left,
            TextWrapping.Wrap, new Size(this._viewbox.DesiredSize.Width, this._viewbox.DesiredSize.Height));

        Rect rect = new Rect(new Size(text.Bounds.Width, text.Bounds.Height));
        return rect;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        var viewBox = e.NameScope.Find("PART_Viewbox");
        var border = e.NameScope.Find("PART_BackgroundBorder");
        
        if (viewBox is Viewbox)
        {
            Viewbox v = ((Viewbox)viewBox);
            this._viewbox = v;
        }
        
        if (border is Border)
        {
            Border b = ((Border)border);
            this._border = b;
        }
    }

    public double Percentage
    {
        get => GetValue(PercentageProperty);
        set
        {
            SetValue(PercentageProperty, value);
        }
    }

    public override void Render(DrawingContext context)
    {
        if(!(DataValidator.ValidateData(this._viewbox) || DataValidator.ValidateData(this._border)))
            return;
        
        if (this._current)
        {
            double realSize = (this.GetBounds("♪").Width * 3) + (3 * 8) + 8;
            
            double scaled = Math.Round(((realSize) / 100) * this.Percentage);
            double scaledB = Math.Round(((realSize) / 100) * this.Percentage);

            this._viewbox.Width = scaled;
            this._viewbox.MaxWidth = scaled;
        
            this._border.Width = scaledB;
            this._border.MaxWidth = scaledB;
            
            return;
        }
        else
        {
            this._viewbox.Width = -10;
            this._viewbox.MaxWidth = -10;

            this._border.Width = -10;
            this._border.MaxWidth = -10;
        }

        base.Render(context);
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

    public bool Current
    {
        get { return this._current; }
        set
        {
            this._current = value;
            SetAndRaise(CurrentProperty, ref _current, value);
        }
    }
}