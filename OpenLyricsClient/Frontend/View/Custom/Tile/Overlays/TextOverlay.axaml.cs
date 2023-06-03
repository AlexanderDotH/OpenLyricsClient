using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using DevBase.Generics;
using DynamicData;
using OpenLyricsClient.Backend;
using OpenLyricsClient.Backend.Events.EventArgs;
using OpenLyricsClient.Backend.Romanization;
using OpenLyricsClient.Backend.Settings.Sections.Lyrics;
using OpenLyricsClient.Frontend.Structure;
using OpenLyricsClient.Frontend.Utils;
using OpenLyricsClient.Shared.Structure.Lyrics;
using OpenLyricsClient.Shared.Structure.Visual;
using OpenLyricsClient.Shared.Utils;
using Org.BouncyCastle.Asn1.X509.Qualified;
using Romanization = OpenLyricsClient.Backend.Romanization.Romanization;

namespace OpenLyricsClient.Frontend.View.Custom.Tile.Overlays;

public partial class TextOverlay : UserControl
{
    public static readonly DirectProperty<TextOverlay, LyricPart> LyricPartProperty = 
        AvaloniaProperty.RegisterDirect<TextOverlay, LyricPart>(nameof(LyricPart), o => o.LyricPart, (o, v) => o.LyricPart = v);
    
    public static StyledProperty<Thickness> LyricMarginProperty =
        AvaloniaProperty.Register<LyricsTile, Thickness>(nameof(LyricMargin));
    
    public static readonly DirectProperty<TextOverlay,  ObservableCollection<LyricOverlayElement>> LyricLinesProperty = 
        AvaloniaProperty.RegisterDirect<TextOverlay,  ObservableCollection<LyricOverlayElement>>(nameof(LyricLines), 
            o => o.LyricLines, 
            (o, v) => o.LyricLines = v);

    private Backend.Romanization.Romanization _romanization;
    
    private LyricPart _lyricPart;
    private ItemsControl _itemsControl;

    private Thickness _lyricMargin;
    private Thickness _lyricsMargin;
    
    private ObservableCollection<LyricOverlayElement> _lines;
    private Typeface _typeface;

    private bool _initialized;

    public TextOverlay()
    {
        InitializeComponent();

        this._initialized = false;
        
        this._lines = new ObservableCollection<LyricOverlayElement>();
        
        this._typeface = new Typeface(FontFamily.Parse(
                "avares://Material.Styles/Fonts/Roboto#Roboto"),
            FontStyle.Normal, this.LyricsWeight);

        this.LyricMargin = new Thickness(0, 0, 0, 5);

        this._romanization = new Backend.Romanization.Romanization();
        
        NewLyricsScroller.Instance.EffectiveViewportChanged += InstanceOnEffectiveViewportChanged;
        Core.INSTANCE.LyricHandler.LyricsFound += LyricHandlerOnLyricsFound;
        Core.INSTANCE.LyricHandler.LyricsPercentageUpdated += LyricHandlerOnLyricsPercentageUpdated;

        Core.INSTANCE.SettingsHandler.SettingsChanged += SettingsHandlerOnSettingsChanged;
        
        this._lyricPart = new LyricPart(-9999, "Hello there ;)");
    }

    private void SettingsHandlerOnSettingsChanged(object sender, SettingsChangedEventArgs settingschangedeventargs)
    {
        
    }

    private void LyricHandlerOnLyricsPercentageUpdated(object sender, LyricsPercentageUpdatedEventArgs args)
    {
        if (args.LyricPart.Equals(this._lyricPart))
        {
            CalculatePercentage(args.Percentage);
        }
        else
        {
            ResetWidths();
        }
    }

    private void LyricHandlerOnLyricsFound(object sender, LyricsFoundEventArgs args)
    {
    }

    private void InstanceOnEffectiveViewportChanged(object? sender, EffectiveViewportChangedEventArgs e)
    {
        UpdateView(NewLyricsScroller.Instance.Bounds.Width, e.EffectiveViewport.Height);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public void UpdateView(double width, double height)
    {
        UpdateTextWrappingLines(this._lyricPart.Part, width, height);
    }
    
    private void UpdateTextWrappingLines(string text, double width, double height)
    {
        ObservableCollection<LyricOverlayElement> sizedLines = new ObservableCollection<LyricOverlayElement>();

        Task.Factory.StartNew(async () =>
        {
            AList<LyricOverlayElement> lines = StringUtils.SplitTextToLines(
                await this._romanization.Romanize(text),
                width - 100,
                height,
                this._typeface,
                this.LyricsAlignment,
                this.LyricsSize);

            sizedLines.AddRange(lines.GetAsList());
        }).GetAwaiter().GetResult();
        
        SetAndRaise(LyricLinesProperty, ref _lines, sizedLines);
    }

    private void CalculatePercentage(double percentage)
    {
        double full = 0;
        double mod = 10;
        double remainder = 0;

        for (var i = 0; i < this._lines.Count; i++)
        {
            LyricOverlayElement element = this._lines[i];
            double width = element.Rect.Width + mod;
            full += width;

            if (i == this._lines.Count - 1)
                remainder = (full * 0.01) * percentage;

            if (width >= remainder && remainder > 0)
            {
                element.Width = remainder;
                remainder = 0;
            }
            else if (remainder > 0)
            {
                element.Width = width;
                remainder -= width;
            }
        }
    }
    
    private void ResetWidths()
    {
        for (var i = 0; i < this._lines.Count; i++)
        {
            this._lines[i].Width = 0;
        }
    }

    private Rect MeasureSingleString(string line, TextWrapping wrapping = TextWrapping.NoWrap)
    {
        FormattedText formattedCandidateLine = new FormattedText(
            line, 
            this._typeface, 
            this.LyricsSize, 
            this.LyricsAlignment, 
            wrapping, 
            new Size(double.PositiveInfinity, double.PositiveInfinity));

        return formattedCandidateLine.Bounds;
    }
    
    public LyricPart LyricPart
    {
        get { return this._lyricPart; }
        set
        {
            if (value == null)
                return;

            if (value.Equals(_lyricPart))
                return;

            SetAndRaise(LyricPartProperty, ref _lyricPart, value);
            
            UpdateTextWrappingLines(this._lyricPart.Part, NewLyricsScroller.Instance.Bounds.Width,
                double.PositiveInfinity);
            
            this._initialized = true;
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
    
    public ObservableCollection<LyricOverlayElement> LyricLines
    {
        get { return this._lines; }
        set
        {
            SetAndRaise(LyricLinesProperty, ref _lines, value);
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
    
    public Size Size
    {
        get
        {
            double width = 0;
            double height = 0;
            
            for (var i = 0; i < this._lines.Count; i++)
            {
                width += Math.Max(width, this._lines[i].Rect.Width);
                height += this._lines[i].Rect.Height + 5;
            }

            return new Size(width, height);
        }
    }
}