using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using OpenLyricsClient.Backend;
using OpenLyricsClient.Backend.Events.EventArgs;
using OpenLyricsClient.Backend.Settings.Sections.Lyrics;
using OpenLyricsClient.Frontend.Models.Pages.Settings;
using OpenLyricsClient.Frontend.Utils;
using OpenLyricsClient.Shared.Structure.Lyrics;

namespace OpenLyricsClient.Frontend.View.Custom.Tile.Overlays;

public partial class NoteOverlay : UserControl, INotifyPropertyChanged
{
    public static readonly DirectProperty<NoteOverlay, LyricPart> LyricPartProperty = 
        AvaloniaProperty.RegisterDirect<NoteOverlay, LyricPart>(nameof(LyricPart), o => o.LyricPart, (o, v) => o.LyricPart = v);
    
    public static StyledProperty<Thickness> LyricMarginProperty =
        AvaloniaProperty.Register<NoteOverlay, Thickness>(nameof(LyricMargin));
    
    public static readonly DirectProperty<NoteOverlay, TimeSpan> AnimationTimeSpanProperty =
        AvaloniaProperty.RegisterDirect<NoteOverlay, TimeSpan>(
            nameof(TimeSpan), 
            o => o.AnimationTimeSpan, 
            (o, v) => o.AnimationTimeSpan = v);
    
    private LyricPart _lyricPart;
    private Thickness _lyricMargin;
    private TimeSpan _animationTimeSpan;
    private double _percentage;
    private double _height;

    private Typeface _typeface;

    private StackPanel _stackPanel;

    private Size _size;
    
    public event PropertyChangedEventHandler? PropertyChanged;

    public NoteOverlay()
    {
        AnimationTimeSpan = TimeSpan.FromSeconds(3);

        AvaloniaXamlLoader.Load(this);

        this._stackPanel = this.Get<StackPanel>(nameof(PART_StackPanel));

        this._percentage = 0;
        
        this._typeface = new Typeface(FontFamily.Parse(
                "avares://Material.Styles/Fonts/Roboto#Roboto"),
            FontStyle.Normal, this.LyricsWeight);

        this._size = CalculateSize();        
        this._height = this._size.Height + 15;
        
        Core.INSTANCE.LyricHandler.LyricsPercentageUpdated += LyricHandlerOnLyricsPercentageUpdated;
        Core.INSTANCE.SettingsHandler.SettingsChanged += SettingsHandlerOnSettingsChanged;
    }

    private void SettingsHandlerOnSettingsChanged(object sender, SettingsChangedEventArgs args)
    {
        if (!args.Section.Equals(typeof(SettingsLyricsViewModel)))
            return;

        this._size = CalculateSize();
        this._height = this._size.Height + 15;
    }

    private Size CalculateSize()
    {
        Rect r = StringUtils.MeasureSingleString(
            "♪", 
            double.PositiveInfinity, 
            double.PositiveInfinity, 
            this._typeface,
            this.LyricsAlignment, this.LyricsSize);

        int amount = 3;
        double spacing = 10 * amount;
        double margin = 10;
        double elements = (r.Width * amount) + margin + spacing;

        return new Size(elements, r.Height);
    }

    private void LyricHandlerOnLyricsPercentageUpdated(object sender, LyricsPercentageUpdatedEventArgs args)
    {
        if (this._lyricPart.Equals(args.LyricPart))
        {
            this.Percentage = CalculateWidthPercentage(args.Percentage);
        }
        else
        {
            this.Percentage = 0;
        }
    }

    public double CalculateWidthPercentage(double percentage)
    {
        double w = this._size.Width;
        double p = (w * 0.01) * percentage;
        return p;
    }
    
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
    
    public LyricPart LyricPart
    {
        get { return this._lyricPart; }
        set
        {
            if (value == null)
                return;

            if (value.Equals(this._lyricPart))
                return;

            SetAndRaise(LyricPartProperty, ref this._lyricPart, value);
        }
    }
    
    public Thickness LyricMargin
    {
        get { return this._lyricMargin; }
        set
        {
            SetAndRaise(LyricMarginProperty, ref _lyricMargin, value);
        }
    }
    
    public SolidColorBrush SelectedLineBrush
    {
        get
        {
            if (Core.INSTANCE.SettingsHandler.Settings<LyricsSection>()!.GetValue<bool>("Artwork Background"))
                return App.Current.FindResource("SelectedLineFontColorBrush") as SolidColorBrush;
            
            return App.Current.FindResource("PrimaryThemeColorBrush") as SolidColorBrush;
        }
    }
    
    public SolidColorBrush UnSelectedLineBrush
    {
        get
        {
            if (Core.INSTANCE.SettingsHandler.Settings<LyricsSection>()!.GetValue<bool>("Artwork Background"))
                return App.Current.FindResource("UnSelectedLineFontColorBrush") as SolidColorBrush;
            
            return SolidColorBrush.Parse("#646464");
        }
    }

    public double LyricsSize
    {
        get => Core.INSTANCE.SettingsHandler.Settings<LyricsSection>().GetValue<double>("Lyrics Size");
    }
    
    public FontWeight LyricsWeight 
    {
        get => Core.INSTANCE.SettingsHandler.Settings<LyricsSection>().GetValue<FontWeight>("Lyrics Weight");
    }
    
    public TextAlignment LyricsAlignment 
    {
        get => Core.INSTANCE.SettingsHandler.Settings<LyricsSection>().GetValue<TextAlignment>("Lyrics Alignment");
    }
    
    public TimeSpan AnimationTimeSpan
    {
        get { return _animationTimeSpan; }
        set
        {
            SetAndRaise(AnimationTimeSpanProperty, ref _animationTimeSpan, value);
        }
    }
    
    public double Percentage
    {
        get { return _percentage; }
        set
        {
            SetField(ref _percentage, value);
        }
    }
    
    public double AnimationHeight
    {
        get { return _height; }
        set
        {
            SetField(ref _height, value);
        }
    }
    
    public Size Size
    {
        get
        {
            return this._size;
        }
    }
}