using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using DevBase.Generics;
using OpenLyricsClient.Backend;
using OpenLyricsClient.Backend.Events.EventArgs;
using OpenLyricsClient.Backend.Settings.Sections.Lyrics;
using OpenLyricsClient.Frontend.Models.Custom.Tile.Overlays;
using OpenLyricsClient.Frontend.Structure;
using OpenLyricsClient.Frontend.Utils;
using OpenLyricsClient.Shared.Structure.Lyrics;
using OpenLyricsClient.Shared.Utils;
using Org.BouncyCastle.Asn1.X509.Qualified;

namespace OpenLyricsClient.Frontend.View.Custom.Tile.Overlays;

public partial class TextOverlay : UserControl
{
    public static readonly DirectProperty<TextOverlay, LyricPart> LyricPartProperty = 
        AvaloniaProperty.RegisterDirect<TextOverlay, LyricPart>(nameof(LyricPart), o => o.LyricPart, (o, v) => o.LyricPart = v);

    public static readonly DirectProperty<TextOverlay,  ObservableCollection<LyricOverlayElement>> LyricLinesProperty = 
        AvaloniaProperty.RegisterDirect<TextOverlay,  ObservableCollection<LyricOverlayElement>>(nameof(LyricLines), 
            o => o.LyricLines, 
            (o, v) => o.LyricLines = v);
    
    private LyricPart _lyricPart;
    private ItemsControl _itemsControl;
    
    private ObservableCollection<LyricOverlayElement> _lines;
    private Typeface _typeface;

    private bool _initialized;

    private Rect _size;
    
    public TextOverlay()
    {
        InitializeComponent();

        this._initialized = false;

        this._size = new Rect();
        
        this._lines = new ObservableCollection<LyricOverlayElement>();
        
        this._typeface = new Typeface(FontFamily.Parse(
                "avares://Material.Styles/Fonts/Roboto#Roboto"),
            FontStyle.Normal, this.LyricsWeight);

        NewLyricsScroller.Instance.EffectiveViewportChanged += InstanceOnEffectiveViewportChanged;
        Core.INSTANCE.LyricHandler.LyricsFound += LyricHandlerOnLyricsFound;
        Core.INSTANCE.LyricHandler.LyricsPercentageUpdated += LyricHandlerOnLyricsPercentageUpdated;

        this._lyricPart = new LyricPart(-9999, "Hello there ;)");
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
       // measuredLinesCache.Clear();
        UpdateView(e.EffectiveViewport.Width, e.EffectiveViewport.Height);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

    }

    public void UpdateView(double width, double height)
    {
        UpdateTextWrappingLines(this._lyricPart.Part, width, height);
    }
    
    private void UpdateTextWrappingLines(string text, double width, double height)
    {
        AList<string> lines = StringUtils.SplitTextToLines(
            text,
            width - 100,
            height,
            this._typeface,
            this.LyricsAlignment,
            this.LyricsSize);

        if (lines.Length > 2)
        {
            lines.ForEach(t=> Debug.WriteLine(t));
        }
        
        ObservableCollection<LyricOverlayElement> sizedLines = new ObservableCollection<LyricOverlayElement>();

        for (int i = 0; i < lines.Length; i++)
        {
            string l = lines.Get(i);
            
            LyricOverlayElement element = new LyricOverlayElement
            {
                Rect = MeasureSingleString(l),
                Line = l
            };
            sizedLines.Add(element);
        }
        
        SetAndRaise(LyricLinesProperty, ref _lines, sizedLines);
    }

    private void CalculatePercentage(double percentage)
    {
        double full = 0;

        for (var i = 0; i < this._lines.Count; i++)
        {
            LyricOverlayElement element = this._lines[i];
            full += element.Rect.Width;
        }

        double remainder = (full * 0.01) * percentage;

        for (var i = 0; i < this._lines.Count; i++)
        {
            LyricOverlayElement element = this._lines[i];

            /*if (remainder <= 0)
            {
                element.Width = element.Rect.Width;
                continue;
            }*/

            if (element.Rect.Width >= remainder)
            {
                element.Width = remainder;
                remainder = 0;
            }
            else
            {
                element.Width = element.Rect.Width;
                remainder -= element.Rect.Width;
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
    
    public ObservableCollection<LyricOverlayElement> LyricLines
    {
        get { return this._lines; }
        set
        {
            SetAndRaise(LyricLinesProperty, ref _lines, value);
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
    
    public Thickness LyricsMargin 
    {
        get => Core.INSTANCE.SettingsHandler.Settings<LyricsSection>().GetValue<Thickness>("Lyrics Margin");
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
                height += this._lines[i].Rect.Height;
            }

            return new Size(width, height);
        }
    }
}