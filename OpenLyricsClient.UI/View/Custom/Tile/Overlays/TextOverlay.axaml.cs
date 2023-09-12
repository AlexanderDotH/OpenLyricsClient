using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using DevBase.Generics;
using DynamicData;
using OpenLyricsClient.Logic;
using OpenLyricsClient.Logic.Events.EventArgs;
using OpenLyricsClient.Logic.Handler.Services.Services;
using OpenLyricsClient.Logic.Settings.Sections.Lyrics;
using OpenLyricsClient.Shared.Structure.Enum;
using OpenLyricsClient.Shared.Structure.Lyrics;
using OpenLyricsClient.Shared.Utils;
using OpenLyricsClient.UI.Events.EventArgs;
using OpenLyricsClient.UI.Extensions;
using OpenLyricsClient.UI.Models.Pages.Settings;
using OpenLyricsClient.UI.Structure;
using OpenLyricsClient.UI.Utils;
using OpenLyricsClient.UI.View.Pages;
using OpenLyricsClient.UI.View.Windows;
using Squalr.Engine.Utils.Extensions;

namespace OpenLyricsClient.UI.View.Custom.Tile.Overlays;

public partial class TextOverlay : UserControl, INotifyPropertyChanged
{
    public static readonly DirectProperty<TextOverlay, LyricPart> LyricPartProperty = 
        AvaloniaProperty.RegisterDirect<TextOverlay, LyricPart>(nameof(LyricPart), o => o.LyricPart, (o, v) => o.LyricPart = v);
    
    public static readonly StyledProperty<Thickness> LyricsMarginProperty = 
        AvaloniaProperty.Register<TextOverlay, Thickness>("LyricsMargin");
    
    public static readonly DirectProperty<TextOverlay,  ObservableCollection<LyricOverlayElement>> LyricLinesProperty = 
        AvaloniaProperty.RegisterDirect<TextOverlay,  ObservableCollection<LyricOverlayElement>>(nameof(LyricLines), 
            o => o.LyricLines, 
            (o, v) => o.LyricLines = v);

    public event PropertyChangedEventHandler? PropertyChanged;
    
    private Logic.Romanization.Romanization _romanization;
    
    private LyricPart _lyricPart;
    private ItemsControl _itemsControl;

    private Thickness _lyricMargin;
    private Thickness _lyricsMargin;

    private Rect _viewPort;
    
    private ObservableCollection<LyricOverlayElement> _lines;
    private Typeface _typeface;

    private bool _initialized;

    private bool _isPointerOver;
    
    private bool _headlessMode;
    private bool _suppressActivity;

    private readonly int LEFT_SPACE;

    private bool _selectedElement;
    private double _percentage;

    public TextOverlay()
    {
        AvaloniaXamlLoader.Load(this);

        LEFT_SPACE = 50;
        
        this._initialized = false;
        this.Headless = false;
        this.SuppressActivity = false;

        this._isPointerOver = false;
        this._selectedElement = false;

        this._percentage = 0;

        this._viewPort = new Rect(0, 0, 900, 900);
        
        AffectsMeasure<TextOverlay>(LyricLinesProperty, LyricPartProperty, LyricsMarginProperty);
        
        this._lines = new ObservableCollection<LyricOverlayElement>();
        
        this._typeface = new Typeface(FontFamily.Parse(
                "avares://Material.Styles/Fonts/Roboto#Roboto"),
            FontStyle.Normal, this.LyricsWeight);

        this.LyricMargin = new Thickness(0, 0, 0, 5);
        
        this._romanization = new Logic.Romanization.Romanization();
        
        LyricsScroller.Instance.EffectiveViewportChanged += InstanceOnEffectiveViewportChanged;
        Core.INSTANCE.LyricHandler.LyricsPercentageUpdated += LyricHandlerOnLyricsPercentageUpdated;
        Core.INSTANCE.LyricHandler.LyricChanged += LyricHandlerOnLyricChanged;
        Core.INSTANCE.SettingsHandler.SettingsChanged += SettingsHandlerOnSettingsChanged;

        this._lyricPart = new LyricPart(-9999, "Hello there ;)");
        
        MainWindow.Instance.PageSelectionChanged += InstanceOnPageSelectionChanged;
        MainWindow.Instance.PageSelectionChangedFinished += InstanceOnPageSelectionChangedFinished;
    }

    private void SettingsHandlerOnSettingsChanged(object sender, SettingsChangedEventArgs settingschangedeventargs)
    {
        if (settingschangedeventargs.Field.Equals("Selections"))
        {
            UpdateView(this._viewPort.Height);
        }
        
        if (settingschangedeventargs.Field.Equals("Selection Mode"))
        {
            OnPropertyChanged("IsFade");
            OnPropertyChanged("IsKaraoke");
            OnPropertyChanged("IsSolid");
        }
    }

    private void LyricHandlerOnLyricChanged(object sender, LyricChangedEventArgs lyricchangedeventargs)
    {
        this._selectedElement = lyricchangedeventargs.LyricPart.Equals(this._lyricPart);
    }

    private void UpdateView(double height)
    {
        if (this.SuppressActivity)
            return;
        
        UpdateTextWrappingLines(this._lyricPart.Part, LyricsScroller.Instance.Bounds.Width - LEFT_SPACE, height);
    }
    
    private void UpdateTextWrappingLines(string text, double width, double height)
    {
        Dispatcher.UIThread.InvokeAsync(async() =>
        {
            ObservableCollection<LyricOverlayElement> sizedLines = new ObservableCollection<LyricOverlayElement>();
            
            AList<LyricOverlayElement> lines = await StringUtils.SplitTextToLines(
                await this._romanization.Romanize(text),
                width,
                height,
                this._typeface,
                this.LyricsAlignment,
                this.LyricsSize, 
                this._romanization);

            sizedLines.AddRange(lines.GetAsList());
            
            SetAndRaise(LyricLinesProperty, ref _lines, sizedLines);
        });
    }

    private void CalculatePercentage(double percentage)
    {
        if (this.Headless)
            return;
        
        if (this.SuppressActivity)
            return;
        
        double full = 0;
        double mod = 10;

        for (var i = 0; i < this._lines.Count; i++)
        {
            LyricOverlayElement element = this._lines[i];
            full += element.Rect.Width + mod;
        }

        double remainder = (full / 100) * percentage;

        Dispatcher.UIThread.InvokeAsync(() =>
        {
            for (var i = 0; i < this._lines.Count; i++)
            {
                LyricOverlayElement element = this._lines[i];
            
                double width = element.Rect.Width + mod;
            
                if (width >= remainder)
                {
                    element.Width = remainder;
                    
                    double per = 100 / remainder * element.Width;
                    element.PercentageMargin = per;
                    
                    remainder = 0;
                }
                else
                {
                    element.Width = width;
                    
                    double per = 100 / width * element.Width;
                    element.PercentageMargin = per;
                    
                    remainder -= width;
                }
                
                element.Percentage = percentage;
                
                element.Selected = this._selectedElement;
            }
        });
    }

    private void ResetDefaults()
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            for (var i = 0; i < this._lines.Count; i++)
            {
                this._lines[i].Width = 0;
                this._lines[i].Percentage = 100;
                this._lines[i].Selected = false;
            }
        });
    }

    #region Events

    private void LyricHandlerOnLyricsPercentageUpdated(object sender, LyricsPercentageUpdatedEventArgs args)
    {
        if (this._headlessMode)
            return;

        if (args.LyricPart.Equals(this._lyricPart))
        {
            this._percentage = args.Percentage;
            CalculatePercentage(args.Percentage);
            OnPropertyChanged("SelectedLineBrush");
        }
        else
        {
            this._percentage = 0;
            ResetDefaults();
        }
    }

    private void InstanceOnEffectiveViewportChanged(object? sender, EffectiveViewportChangedEventArgs e)
    {
        this._viewPort = e.EffectiveViewport;
        UpdateView(e.EffectiveViewport.Height);
    }
    
    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        IService service = Core.INSTANCE.ServiceHandler.GetActiveService();

        if (!DataValidator.ValidateData(service))
            return;

        if (service.CanSeek())
        {
            LyricsScroller.Instance.AllowSync();
            Task.Factory.StartNew(async () => await service.Seek(this._lyricPart.Time));
        }
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
        
        this._lines.ForEach(a => a.PointerOver = true);
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

        this._lines.ForEach(a => a.PointerOver = false);
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
            
            UpdateTextWrappingLines(this._lyricPart.Part, LyricsScroller.Instance.Bounds.Width - LEFT_SPACE,
                double.PositiveInfinity);
            
            this._initialized = true;
        }
    }
    
    public Thickness LyricMargin
    {
        get => GetValue(LyricsMarginProperty);
        set => SetValue(LyricsMarginProperty, value);
    }
    
    public ObservableCollection<LyricOverlayElement> LyricLines
    {
        get { return this._lines; }
        set
        {
            SetAndRaise(LyricLinesProperty, ref _lines, value);
        }
    }
    
    public SolidColorBrush MergedLineBrush { get; set; }
    
    public SolidColorBrush SelectedLineBrush
    {
        get
        {
            SolidColorBrush colorBrush = App.Current.FindResource("PrimaryThemeColorBrush") as SolidColorBrush;
            
            if (Core.INSTANCE.SettingsHandler.Settings<LyricsSection>()!.GetValue<bool>("Artwork Background"))
                colorBrush = App.Current.FindResource("SelectedLineFontColorBrush") as SolidColorBrush;

            return colorBrush;
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

    public bool IsKaraoke
    {
        get => Core.INSTANCE.SettingsHandler.Settings<LyricsSection>()
            .GetValue<EnumLyricsDisplayMode>("Selection Mode") == EnumLyricsDisplayMode.KARAOKE;
    }
    
    public bool IsFade
    {
        get => Core.INSTANCE.SettingsHandler.Settings<LyricsSection>()
            .GetValue<EnumLyricsDisplayMode>("Selection Mode") == EnumLyricsDisplayMode.FADE;
    }
    
    public bool IsSolid
    {
        get => Core.INSTANCE.SettingsHandler.Settings<LyricsSection>()
            .GetValue<EnumLyricsDisplayMode>("Selection Mode") == EnumLyricsDisplayMode.SOLID;
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
            return this.MeasureOverride(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            
            
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