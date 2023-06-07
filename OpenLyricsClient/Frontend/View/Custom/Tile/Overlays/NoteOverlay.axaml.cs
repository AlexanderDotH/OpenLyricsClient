using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using DevBase.Generics;
using OpenLyricsClient.Backend;
using OpenLyricsClient.Backend.Events.EventArgs;
using OpenLyricsClient.Backend.Handler.Services.Services;
using OpenLyricsClient.Backend.Settings.Sections.Lyrics;
using OpenLyricsClient.Frontend.Events.EventArgs;
using OpenLyricsClient.Frontend.Extensions;
using OpenLyricsClient.Frontend.Models.Pages.Settings;
using OpenLyricsClient.Frontend.Structure.Enum;
using OpenLyricsClient.Frontend.Utils;
using OpenLyricsClient.Frontend.View.Pages;
using OpenLyricsClient.Frontend.View.Windows;
using OpenLyricsClient.Shared.Structure.Lyrics;
using OpenLyricsClient.Shared.Utils;
using Squalr.Engine.Utils.Extensions;

namespace OpenLyricsClient.Frontend.View.Custom.Tile.Overlays;

public partial class NoteOverlay : UserControl, INotifyPropertyChanged
{
    public static readonly DirectProperty<NoteOverlay, LyricPart> LyricPartProperty = 
        AvaloniaProperty.RegisterDirect<NoteOverlay, LyricPart>(nameof(LyricPart), o => o.LyricPart, (o, v) => o.LyricPart = v);
    
    public static StyledProperty<Thickness> LyricMarginProperty =
        AvaloniaProperty.Register<NoteOverlay, Thickness>(nameof(LyricMargin));
    
    private LyricPart _lyricPart;
    private Thickness _lyricMargin;
    private double _percentage;
    private double _height;
    
    private bool _animate;
    private double _speed;
    private AList<(string, Avalonia.Animation.Animation)> _animatale;

    private TimeSpan _idleTimeSpan;
    private TimeSpan _noteTimeSpan;

    private Typeface _typeface;

    private Size _size;

    private bool _headlessMode;
    private bool _isPointerOver;
    
    public event PropertyChangedEventHandler? PropertyChanged;

    public NoteOverlay()
    {
        this._speed = 0;

        this._percentage = 0;

        this.Headless = false;

        this._isPointerOver = false;
        
        this._animatale = new AList<(string, Avalonia.Animation.Animation)>();
        this._idleTimeSpan = TimeSpan.FromSeconds(2);
        this._noteTimeSpan = TimeSpan.FromSeconds(3);
        
        this._typeface = new Typeface(FontFamily.Parse(
                "avares://Material.Styles/Fonts/Roboto#Roboto"),
            FontStyle.Normal, this.LyricsWeight);
        
        this._size = CalculateSize();
        this._height = this._size.Height + 15;
        
        Core.INSTANCE.LyricHandler.LyricsPercentageUpdated += LyricHandlerOnLyricsPercentageUpdated;
        Core.INSTANCE.LyricHandler.LyricsFound += LyricHandlerOnLyricsFound;
        Core.INSTANCE.SettingsHandler.SettingsChanged += SettingsHandlerOnSettingsChanged;
        
        AvaloniaXamlLoader.Load(this);
        
        ApplyAnimationToClasses(this._idleTimeSpan, this._noteTimeSpan);
        
        MainWindow.Instance.PageSelectionChanged += InstanceOnPageSelectionChanged;
        MainWindow.Instance.PageSelectionChangedFinished += InstanceOnPageSelectionChangedFinished;
    }

    #region Events

    private void LyricHandlerOnLyricsFound(object sender, LyricsFoundEventArgs lyricsfoundeventargs)
    {
        if (this.Headless)
            return;

        Dispatcher.UIThread.InvokeAsync(() =>
        {
            Speed = lyricsfoundeventargs.LyricData.LyricSpeed;
        });
    }

    private void SettingsHandlerOnSettingsChanged(object sender, SettingsChangedEventArgs args)
    {
        if (this.Headless)
            return;
        
        if (!args.Section.Equals(typeof(SettingsLyricsViewModel)))
            return;

        this._size = CalculateSize();
        this._height = this._size.Height + 15;
    }

    private void LyricHandlerOnLyricsPercentageUpdated(object sender, LyricsPercentageUpdatedEventArgs args)
    {
        if (this.Headless)
            return;
        
        if (this._lyricPart.Equals(args.LyricPart))
        {
            this.Percentage = CalculateWidthPercentage(args.Percentage);
            Animate = true;
        }
        else
        {
            this.Percentage = 0;
            Animate = false;
        }
    }
    
    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        MainWindow.Instance.WindowDragable = false;
        
        IService service = Core.INSTANCE.ServiceHandler.GetActiveService();

        if (!DataValidator.ValidateData(service))
            return;
        
        if (service.CanSeek())
            Task.Factory.StartNew(async () => await service.Seek(this._lyricPart.Time));
        
        NewLyricsScroller.Instance.Resync(this.LyricPart);
    }    
    
    private void InputElement_OnPointerEnter(object? sender, PointerEventArgs e)
    {
        MainWindow.Instance.WindowDragable = true;
        
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
        }
    }
    
    private void InstanceOnPageSelectionChangedFinished(object sender, PageSelectionChangedEventArgs pageselectionchanged)
    {
        if (pageselectionchanged.ToPage.GetType() == typeof(LyricsPage))
        {
            this.Headless = false;
        }
    }

    #endregion

    #region Animations

    private void ApplyAnimationToClasses(TimeSpan idleTimeSpan, TimeSpan noteTimeSpan)
    {
        if (this.Headless)
            return;
        
        Styles styles = new Styles();

        double modifier = 0.8d;
        
        for (int i = 1; i <= 3; i++)
        {
            styles.Add(IdleAnimationStyle($"idle{i}", idleTimeSpan, TimeSpan.FromSeconds(modifier * (i - 1))));
        }
        
        for (int i = 1; i <= 3; i++)
        {
            styles.Add(ActiveAnimationStyle($"note{i}", noteTimeSpan, TimeSpan.FromSeconds(modifier *(i - 1))));
        }
        
        this.Styles.Add(styles);
    }
    
    private Style IdleAnimationStyle(string className, TimeSpan duration, TimeSpan delay)
    {
        var animation = new Avalonia.Animation.Animation
        {
            Duration = duration,
            Delay = delay,
            IterationCount = IterationCount.Infinite,
            Easing = new CircularEaseInOut(),
            FillMode = FillMode.Both,
            PlaybackDirection = PlaybackDirection.Alternate
        };

        var keyFrame1 = new KeyFrame
        {
            Cue = new Cue(0),
            Setters =
            {
                new Setter(TextBlock.OpacityProperty, 0.2)
            }
        };

        var keyFrame2 = new KeyFrame
        {
            Cue = new Cue(1),
            Setters =
            {
                new Setter(TextBlock.OpacityProperty, 1)
            }
        };

        animation.Children.Add(keyFrame1);
        animation.Children.Add(keyFrame2);

        var style = new Style(x => x.OfType<TextBlock>().Class(className));
        style.Animations.Add(animation);

        this._animatale.Add((className, animation));
        
        return style;
    }
    
    private Style ActiveAnimationStyle(string className, TimeSpan duration, TimeSpan delay)
    {
        var animation = new Avalonia.Animation.Animation
        {
            Duration = duration,
            Delay = delay,
            IterationCount = IterationCount.Infinite,
            Easing = new ElasticEaseIn(),
            FillMode = FillMode.Both,
            PlaybackDirection = PlaybackDirection.Alternate
        };

        var keyFrame1 = new KeyFrame
        {
            Cue = new Cue(0),
            Setters = {
                new Setter(TextBlock.MarginProperty, new Thickness(0,0,0,0)),
                new Setter(TextBlock.OpacityProperty, 0.7)
            }
        };

        var keyFrame2 = new KeyFrame
        {
            Cue = new Cue(1),
            Setters = {
                new Setter(TextBlock.MarginProperty, new Thickness(0,0,0,10)),
                new Setter(TextBlock.OpacityProperty, 1)
            }
        };

        animation.Children.Add(keyFrame1);
        animation.Children.Add(keyFrame2);

        var style = new Style(x => x.OfType<TextBlock>().Class(className));
        style.Animations.Add(animation);
        
        this._animatale.Add((className, animation));
        
        return style;
    }
    
    private void ApplyDelay(string classes, TimeSpan span)
    {
        double h = span.TotalMilliseconds / 3;
        double factor = (h / (3 * 6)) * 0.01d;
        
        int position = 0;
        for (int i = 0; i < this._animatale.Length; i++)
        {
            (string, Avalonia.Animation.Animation) element = this._animatale.Get(i);
        
            if (element.Item1.SequenceEqual($"{classes}{position + 1}"))
            {
                element.Item2.Delay = TimeSpan.FromMilliseconds(position * factor);
                position++;
            }
        }
    }

    private void ApplyDuration(string classes, TimeSpan span)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            this._animatale.ForEach(a =>
            {
                if (a.Item1.Contains(classes))
                    a.Item2.Duration = span;
            });
        });
    }

    #endregion
    
    #region Calculatios

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
    
    private double CalculateWidthPercentage(double percentage)
    {
        double w = this._size.Width;
        double p = (w * 0.01) * percentage;
        return p;
    }

    private TimeSpan CalculateSpeedToTimeSpan(double percentage, TimeSpan maxTimeSpan)
    {
        double multiplier = 1.0d;

        double max = maxTimeSpan.TotalMilliseconds;
        double x = percentage * (max * 0.01);
        double y = max - x;

        y = Math.Clamp(y, 0, max);
        y = Math.Abs(y);

        double result = y * multiplier;
        
        return TimeSpan.FromMilliseconds(result);
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

    #region Getter and setter

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
    
    public double Percentage
    {
        get { return _percentage; }
        set
        {
            SetField(ref _percentage, value);
        }
    }
    
    public double Speed
    {
        get { return _speed; }
        set
        {
            SetField(ref _speed, value);

            if (double.IsInfinity(value) || double.IsNegative(value) || double.IsNaN(value))
                return;

            TimeSpan idleSpan = CalculateSpeedToTimeSpan(value, this._idleTimeSpan);
            ApplyDuration("idle", idleSpan);
            ApplyDelay("idle", idleSpan);
            
            TimeSpan noteSpan = CalculateSpeedToTimeSpan(value, this._noteTimeSpan);
            ApplyDuration("note", noteSpan);
            ApplyDelay("note", noteSpan);
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
            return new Size(this._size.Width, this._height);
        }
    }
    
    public bool Animate
    {
        get => this._animate;
        set => this.SetField(ref this._animate, value);
    }
    
    public bool Headless
    {
        get => this._headlessMode;
        set => this.SetField(ref this._headlessMode, value);
    }

    #endregion
}