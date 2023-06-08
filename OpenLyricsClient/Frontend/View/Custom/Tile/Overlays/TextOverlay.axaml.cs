using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using DevBase.Generics;
using DynamicData;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenLyricsClient.Backend;
using OpenLyricsClient.Backend.Events.EventArgs;
using OpenLyricsClient.Backend.Handler.Services.Services;
using OpenLyricsClient.Backend.Romanization;
using OpenLyricsClient.Backend.Settings.Sections.Lyrics;
using OpenLyricsClient.Frontend.Events.EventArgs;
using OpenLyricsClient.Frontend.Extensions;
using OpenLyricsClient.Frontend.Structure;
using OpenLyricsClient.Frontend.Utils;
using OpenLyricsClient.Frontend.View.Pages;
using OpenLyricsClient.Frontend.View.Windows;
using OpenLyricsClient.Shared.Structure.Lyrics;
using OpenLyricsClient.Shared.Structure.Visual;
using OpenLyricsClient.Shared.Utils;
using Org.BouncyCastle.Asn1.X509.Qualified;
using Romanization = OpenLyricsClient.Backend.Romanization.Romanization;

namespace OpenLyricsClient.Frontend.View.Custom.Tile.Overlays;

public partial class TextOverlay : UserControl, INotifyPropertyChanged
{
    public static readonly DirectProperty<TextOverlay, LyricPart> LyricPartProperty = 
        AvaloniaProperty.RegisterDirect<TextOverlay, LyricPart>(nameof(LyricPart), o => o.LyricPart, (o, v) => o.LyricPart = v);
    
    public static StyledProperty<Thickness> LyricMarginProperty =
        AvaloniaProperty.Register<LyricsTile, Thickness>(nameof(LyricMargin));
    
    public static readonly DirectProperty<TextOverlay,  ObservableCollection<LyricOverlayElement>> LyricLinesProperty = 
        AvaloniaProperty.RegisterDirect<TextOverlay,  ObservableCollection<LyricOverlayElement>>(nameof(LyricLines), 
            o => o.LyricLines, 
            (o, v) => o.LyricLines = v);

    public event PropertyChangedEventHandler? PropertyChanged;
    
    private Backend.Romanization.Romanization _romanization;
    
    private LyricPart _lyricPart;
    private ItemsControl _itemsControl;

    private Thickness _lyricMargin;
    private Thickness _lyricsMargin;
    
    private ObservableCollection<LyricOverlayElement> _lines;
    private Typeface _typeface;

    private bool _initialized;

    private bool _isPointerOver;
    
    private bool _headlessMode;
    private bool _suppressActivity;

    private readonly int LEFT_SPACE;
    
    public TextOverlay()
    {
        AvaloniaXamlLoader.Load(this);

        LEFT_SPACE = 50;
        
        this._initialized = false;
        this.Headless = false;
        this.SuppressActivity = false;

        this._isPointerOver = false;
        
        this._lines = new ObservableCollection<LyricOverlayElement>();
        
        this._typeface = new Typeface(FontFamily.Parse(
                "avares://Material.Styles/Fonts/Roboto#Roboto"),
            FontStyle.Normal, this.LyricsWeight);

        this.LyricMargin = new Thickness(0, 0, 0, 5);

        this._romanization = new Backend.Romanization.Romanization();
        
        NewLyricsScroller.Instance.EffectiveViewportChanged += InstanceOnEffectiveViewportChanged;
        Core.INSTANCE.LyricHandler.LyricsPercentageUpdated += LyricHandlerOnLyricsPercentageUpdated;

        this._lyricPart = new LyricPart(-9999, "Hello there ;)");
        
        MainWindow.Instance.PageSelectionChanged += InstanceOnPageSelectionChanged;
        MainWindow.Instance.PageSelectionChangedFinished += InstanceOnPageSelectionChangedFinished;
    }

    private void UpdateView(double width, double height)
    {
        if (this.SuppressActivity)
            return;
        
        UpdateTextWrappingLines(this._lyricPart.Part, NewLyricsScroller.Instance.Bounds.Width - LEFT_SPACE, height);
    }
    
    private void UpdateTextWrappingLines(string text, double width, double height)
    {
        ObservableCollection<LyricOverlayElement> sizedLines = new ObservableCollection<LyricOverlayElement>();

        Task.Factory.StartNew(async () =>
        {
            AList<LyricOverlayElement> lines = StringUtils.SplitTextToLines(
                await this._romanization.Romanize(text),
                width,
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

        for (var i = 0; i < this._lines.Count; i++)
        {
            LyricOverlayElement element = this._lines[i];
            full += element.Rect.Width + mod;
        }

        double remainder = (full / 100) * percentage;

        for (var i = 0; i < this._lines.Count; i++)
        {
            LyricOverlayElement element = this._lines[i];

            double width = element.Rect.Width + mod;
            
            if (width >= remainder)
            {
                element.Width = remainder;
                remainder = 0;
            }
            else
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

    #region Events

    private void LyricHandlerOnLyricsPercentageUpdated(object sender, LyricsPercentageUpdatedEventArgs args)
    {
        if (this._headlessMode)
            return;

        if (args.LyricPart.Equals(this._lyricPart))
        {
            CalculatePercentage(args.Percentage);
        }
        else
        {
            ResetWidths();
        }
    }

    private void InstanceOnEffectiveViewportChanged(object? sender, EffectiveViewportChangedEventArgs e)
    {
        UpdateView(e.EffectiveViewport.Width, e.EffectiveViewport.Height);
    }
    
    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        IService service = Core.INSTANCE.ServiceHandler.GetActiveService();

        if (!DataValidator.ValidateData(service))
            return;
        
        //NewLyricsScroller.Instance.UnSync();
        NewLyricsScroller.Instance.Resync(this.LyricPart);
        
        if (service.CanSeek())
            Task.Factory.StartNew(async () => await service.Seek(this._lyricPart.Time));
    }

    private void InputElement_OnPointerEnter(object? sender, PointerEventArgs e)
    {
        MainWindow.Instance.WindowDragable = false;
        
        IService service = Core.INSTANCE.ServiceHandler.GetActiveService();
        
        if (!DataValidator.ValidateData(service))
            return;
        
        if (!service.CanSeek())
            return;
        
        this._isPointerOver = true;
        OnPropertyChanged("UnSelectedLineBrush");
    }

    private void InputElement_OnPointerLeave(object? sender, PointerEventArgs e)
    {
        MainWindow.Instance.WindowDragable = true;
        
        IService service = Core.INSTANCE.ServiceHandler.GetActiveService();
        
        if (!DataValidator.ValidateData(service))
            return;
        
        if (!service.CanSeek())
            return;
        
        this._isPointerOver = false;
        OnPropertyChanged("UnSelectedLineBrush");
    }    

    private void InstanceOnPageSelectionChanged(object sender, PageSelectionChangedEventArgs pageselectionchanged)
    {
        if (pageselectionchanged.ToPage.GetType() == typeof(SettingsPage))
        {
            this.Headless = true;
            this.SuppressActivity = true;
        }
    }
    
    private void InstanceOnPageSelectionChangedFinished(object sender, PageSelectionChangedEventArgs pageselectionchanged)
    {
        if (pageselectionchanged.ToPage.GetType() == typeof(LyricsPage))
        {
            this.Headless = false;
            this.SuppressActivity = false;
        }
    }
    
    #endregion

    #region MVVM Stuff

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

    #endregion
    
    #region Getter and Setter

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
            
            if (this.SuppressActivity)
                return;
            
            UpdateTextWrappingLines(this._lyricPart.Part, NewLyricsScroller.Instance.Bounds.Width - LEFT_SPACE,
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
            {
                SolidColorBrush colorBrush = App.Current.FindResource("UnSelectedLineFontColorBrush") as SolidColorBrush;

                if (this._isPointerOver)
                    return colorBrush.AdjustBrightness(120);
                
                return colorBrush;
            }

            if (this._isPointerOver)
                return SelectedLineBrush.AdjustBrightness(90);
            
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
    
    public bool Headless
    {
        get => this._headlessMode;
        set => this._headlessMode = value;
    }

    public bool SuppressActivity
    {
        get => _suppressActivity;
        set => _suppressActivity = value;
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

    #endregion
}