using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using OpenLyricsClient.Backend;
using OpenLyricsClient.Backend.Events.EventArgs;
using OpenLyricsClient.Backend.Settings.Sections.Lyrics;
using OpenLyricsClient.Shared.Structure.Lyrics;

namespace OpenLyricsClient.Frontend.View.Custom.Tile.Overlays;

public partial class NoteOverlay : UserControl
{
    public static readonly DirectProperty<NoteOverlay, LyricPart> LyricPartProperty = 
        AvaloniaProperty.RegisterDirect<NoteOverlay, LyricPart>(nameof(LyricPart), o => o.LyricPart, (o, v) => o.LyricPart = v);
    
    public static StyledProperty<Thickness> LyricMarginProperty =
        AvaloniaProperty.Register<NoteOverlay, Thickness>(nameof(LyricMargin));
    
    public static readonly StyledProperty<double> PercentageProperty =
        AvaloniaProperty.Register<NoteOverlay, double>(nameof(Percentage));
    
    public static readonly DirectProperty<NoteOverlay, TimeSpan> AnimationTimeSpanProperty =
        AvaloniaProperty.RegisterDirect<NoteOverlay, TimeSpan>(
            nameof(TimeSpan), 
            o => o.AnimationTimeSpan, 
            (o, v) => o.AnimationTimeSpan = v);
    
    private LyricPart _lyricPart;
    private Thickness _lyricMargin;
    private TimeSpan _animationTimeSpan;

    private StackPanel _stackPanel;

    public NoteOverlay()
    {
        AnimationTimeSpan = TimeSpan.FromSeconds(3);

        AvaloniaXamlLoader.Load(this);

        this._stackPanel = this.Get<StackPanel>(nameof(PART_StackPanel));
        
        Core.INSTANCE.LyricHandler.LyricsPercentageUpdated += LyricHandlerOnLyricsPercentageUpdated;
    }

    private void LyricHandlerOnLyricsPercentageUpdated(object sender, LyricsPercentageUpdatedEventArgs args)
    {
        if (!this._lyricPart.Equals(args.LyricPart))
            return;

        this.Percentage = CalculateWidthPercentage(args.Percentage);
    }

    public double CalculateWidthPercentage(double percentage)
    {
        double w = this._stackPanel.Bounds.Width;
        double p = (w * 0.01) * percentage;
        return p;
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
        get => GetValue(PercentageProperty);
        set => SetValue(PercentageProperty, value);
    }
    
    public Size Size
    {
        get
        {
            return this._stackPanel.Bounds.Size;
        }
    }
}