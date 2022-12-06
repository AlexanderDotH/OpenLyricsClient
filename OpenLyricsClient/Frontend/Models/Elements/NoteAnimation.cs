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
        var border = e.NameScope.Find("PART_BackgroundBorder");
        
        if (border is Border)
        {
            Border b = (Border)border;
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
                SetValue(PercentageProperty, Math.Round(((this.DesiredSize.Width) / 100) * value));
            }
        }
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