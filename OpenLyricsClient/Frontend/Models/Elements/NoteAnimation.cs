using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using DevBase.Generics;
using OpenLyricsClient.Backend;
using OpenLyricsClient.Backend.Structure.Enum;
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

    private AList<TextBlock> _notes;

    public NoteAnimation()
    {
        FontSize = 30;
        FontWeight = FontWeight.Bold;
        this._current = false;

        this._notes = new AList<TextBlock>();
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
                FontStyle.Normal, this.FontWeight), this.FontSize * App.INSTANCE.ScalingManager.CurrentScaling, TextAlignment.Left,
            TextWrapping.Wrap, new Size(this._viewbox.DesiredSize.Width, this._viewbox.DesiredSize.Height));

        Rect rect = new Rect(new Size(text.Bounds.Width, text.Bounds.Height));
        return rect;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        var viewBox = e.NameScope.Find("PART_Viewbox");
        var border = e.NameScope.Find("PART_BackgroundBorder");

        for (int i = 1; i < 6; i++)
        {
            var note = e.NameScope.Find("NOTE_" + i);

            if (note is TextBlock)
            {
                this._notes.Add((TextBlock)note);
            }
        }
        
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
            
            if (Core.INSTANCE.SettingManager.Settings.DisplayPreferences.DisplayMode == EnumLyricsDisplayMode.FADE)
            {
                this._viewbox.IsVisible = false;
                this._border.Width = 0;
                
                Color color = ((SolidColorBrush)this.SelectedLineBrush).Color;
                Color unselectedColor = ((SolidColorBrush)this.UnSelectedLineBrush).Color;

                double percentage = Math.Clamp(this.Percentage / 100.0, 0, 100);
            
                double red = (unselectedColor.R * (1 - percentage) + color.R * percentage);
                double green = (unselectedColor.G * (1 - percentage) + color.G * percentage);
                double blue = (unselectedColor.B * (1 - percentage) + color.B * percentage);

                Color newColor = new Color(
                    255,
                    (byte)Math.Clamp(red, 0, 255), 
                    (byte)Math.Clamp(green, 0, 255), 
                    (byte)Math.Clamp(blue, 0, 255));

                for (int i = 0; i < this._notes.Length; i++)
                {
                    this._notes[i].Foreground = new SolidColorBrush(newColor);
                }
            }
            else
            {
                for (int i = 0; i < this._notes.Length; i++)
                {
                    if (i <= 2)
                    {
                        this._notes[i].Foreground = ((SolidColorBrush)this.UnSelectedLineBrush);
                    }
                    else
                    {
                        this._notes[i].Foreground = ((SolidColorBrush)this.SelectedLineBrush);
                    }
                }
                
                double scaled = Math.Round(((realSize) / 100) * this.Percentage);
                double scaledB = Math.Round(((realSize) / 100) * this.Percentage);

                this._viewbox.Width = scaled;
                this._viewbox.MaxWidth = scaled;
        
                this._border.Width = scaledB;
                this._border.MaxWidth = scaledB;
            }
            
            return;
        }
        else
        {
            for (int i = 0; i < this._notes.Length; i++)
            {
                if (i <= 2)
                {
                    this._notes[i].Foreground = ((SolidColorBrush)this.UnSelectedLineBrush);
                }
                else
                {
                    this._notes[i].Foreground = ((SolidColorBrush)this.SelectedLineBrush);
                }
            }
            
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