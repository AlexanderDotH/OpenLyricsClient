﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using DevBase.Generics;
using DynamicData;
using OpenLyricsClient.Logic;
using OpenLyricsClient.Logic.Events.EventArgs;
using OpenLyricsClient.Logic.Handler.Services.Services;
using OpenLyricsClient.Logic.Settings.Sections.Lyrics;
using OpenLyricsClient.Shared.Structure.Lyrics;
using OpenLyricsClient.Shared.Utils;
using OpenLyricsClient.UI.Events.EventArgs;
using OpenLyricsClient.UI.Extensions;
using OpenLyricsClient.UI.Structure;
using OpenLyricsClient.UI.Utils;
using OpenLyricsClient.UI.View.Pages;
using OpenLyricsClient.UI.View.Windows;

namespace OpenLyricsClient.UI.View.Custom.Tile.Overlays;

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
    
    private Logic.Romanization.Romanization _romanization;
    
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

        this._romanization = new Logic.Romanization.Romanization();
        
        NewLyricsScroller.Instance.EffectiveViewportChanged += InstanceOnEffectiveViewportChanged;
        Core.INSTANCE.LyricHandler.LyricsPercentageUpdated += LyricHandlerOnLyricsPercentageUpdated;

        this._lyricPart = new LyricPart(-9999, "Hello there ;)");
        
        MainWindow.Instance.PageSelectionChanged += InstanceOnPageSelectionChanged;
        MainWindow.Instance.PageSelectionChangedFinished += InstanceOnPageSelectionChangedFinished;
    }

    private void UpdateView(double height)
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
        UpdateView(e.EffectiveViewport.Height);
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
    
    public SolidColorBrush MergedLineBrush { get; set; }
    
    public SolidColorBrush SelectedLineBrush
    {
        get
        {
            SolidColorBrush colorBrush = App.Current.FindResource("PrimaryThemeColorBrush") as SolidColorBrush;
            
            if (Core.INSTANCE.SettingsHandler.Settings<LyricsSection>()!.GetValue<bool>("Artwork Background"))
                colorBrush = App.Current.FindResource("SelectedLineFontColorBrush") as SolidColorBrush;

            /*if (Core.INSTANCE.SettingsHandler
                    .Settings<LyricsSection>()!
                    .GetValue<EnumLyricsDisplayMode>(
                    "Selection Mode") == EnumLyricsDisplayMode.FADE)
            {
                colorBrush.AdjustBrightness(this.)
            }*/

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