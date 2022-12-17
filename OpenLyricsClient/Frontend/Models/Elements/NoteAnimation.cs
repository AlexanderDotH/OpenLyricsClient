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
            if (value < 0)
            {
                SetValue(PercentageProperty, value);
            }
            else
            {
                SetValue(PercentageProperty, value);

                if (this._viewbox == null || this._border == null)
                    return;

                if (!this._current)
                {
                    this._viewbox.Width = -10;
                    this._viewbox.MaxWidth = -10;
        
                    this._border.Width = -10;
                    this._border.MaxWidth = -10;
                    
                    return;
                }
                
                double scaled = Math.Round(((this.DesiredSize.Width) / 100) * value);
                double scaledB = Math.Round(((this.DesiredSize.Width) / 100) * value);


                this._viewbox.Width = scaled;
                this._viewbox.MaxWidth = scaled;
        
                this._border.Width = scaledB;
                this._border.MaxWidth = scaledB;
            }
        }
    }

    public override void Render(DrawingContext context)
    {
       

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