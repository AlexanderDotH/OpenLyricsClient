using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Rendering;
using Avalonia.Threading;
using DevBase.Async.Task;
using OpenLyricsClient.Backend;
using OpenLyricsClient.Backend.Events.EventHandler;
using OpenLyricsClient.Backend.Structure.Enum;
using OpenLyricsClient.Backend.Structure.Lyrics;
using OpenLyricsClient.Backend.Utils;
using OpenLyricsClient.Frontend.Models.Custom;
using OpenLyricsClient.Frontend.Models.Elements;
using OpenLyricsClient.Frontend.View.Windows;
using Squalr.Engine.Utils.Extensions;
using ScrollChangedEventArgs = OpenLyricsClient.Frontend.Models.Custom.ScrollChangedEventArgs;
using TextAlignment = Avalonia.Media.TextAlignment;

namespace OpenLyricsClient.Frontend.View.Custom;

public partial class LyricsScroller : UserControl
{
    public static readonly StyledProperty<int> SelectedLineProperty =
        AvaloniaProperty.Register<LyricsScroller, int>(nameof(SelectedLine));

    public static readonly StyledProperty<Brush> SelectedLineBrushProperty =
        AvaloniaProperty.Register<LyricsScroller, Brush>(nameof(SelectedLineBrush));

    public static readonly StyledProperty<Brush> UnSelectedLineBrushProperty =
        AvaloniaProperty.Register<LyricsScroller, Brush>(nameof(UnSelectedLineBrush));
    
    public static readonly StyledProperty<Thickness> ItemMarginProperty =
        AvaloniaProperty.Register<LyricsScroller, Thickness>(nameof(ItemMargin));
    
    public static readonly DirectProperty<LyricsScroller, LyricPart> LyricPartProperty = 
        AvaloniaProperty.RegisterDirect<LyricsScroller, LyricPart>(nameof(LyricPart), o => o.LyricPart, (o, v) => o.LyricPart = v);
    
    public static readonly DirectProperty<LyricsScroller, ObservableCollection<LyricPart>> LyricPartsProperty = 
        AvaloniaProperty.RegisterDirect<LyricsScroller, ObservableCollection<LyricPart>>(nameof(LyricParts), o => o.LyricParts, (o, v) => o.LyricParts = v);

    public static readonly StyledProperty<FontWeight> LyricsFontWeightProperty =
        AvaloniaProperty.Register<LyricsScroller, FontWeight>(nameof(LyricsFontWeight));
    
    public static readonly StyledProperty<int> LyricsFontSizeProperty =
        AvaloniaProperty.Register<LyricsScroller, int>(nameof(LyricsFontSize));
    
    public static readonly StyledProperty<bool> IsSyncedProperty =
        AvaloniaProperty.Register<LyricsScroller, bool>(nameof(IsSynced));

    private ObservableCollection<LyricPart> _lyricParts;
    private LyricPart _lyricPart;
    private LyricsCard _card;
    
    private double _scrollFrom;
    private double _currentScrollOffset;
    private double _scrollTo;
    private double _oldScrollY;

    private bool _isFirstSync;
    private int _scrollCount;

    private double _startMargin;
    private double _scrollSpeed;
    
    private CustomScrollViewer _scrollViewer;
    private ItemsRepeater _itemsRepeater;

    private Grid _gradientTop;
    private Grid _gradientBottom;
    
    private SleepLoopRenderTimer _renderTimer;

    private LyricsScrollerViewModel _viewModel;
    
    public LyricsScroller()
    {
        InitializeComponent();

        this._viewModel = new LyricsScrollerViewModel();
        this.DataContext = this._viewModel;

        this._scrollViewer = this.Get<CustomScrollViewer>(nameof(CTRL_Viewer));
        this._itemsRepeater = this.Get<ItemsRepeater>(nameof(CTRL_Repeater));
            
        this._gradientTop = this.Get<Grid>(nameof(GradientTop));
        this._gradientBottom = this.Get<Grid>(nameof(GradientBottom));

        this._scrollFrom = 0;
        this._currentScrollOffset = 0;
        this._scrollTo = 0;
        this.IsSynced = true;
        this._isFirstSync = true;
        this._scrollCount = -2;
        this._oldScrollY = 0;
        this._startMargin = 0;
        this._scrollSpeed = 15;

        this._renderTimer = new SleepLoopRenderTimer(150);
        this._renderTimer.Tick += RenderTimerOnTick;

        /*Core.INSTANCE.SettingManager.SettingsChanged  += (sender, args) =>
        {
            Reload();
            Reset();
        };*/
    }
    
    private void RenderTimerOnTick(TimeSpan obj)
    {
        double step = Math.Abs(this._scrollTo - this._currentScrollOffset) / this._scrollSpeed;
        
        if (this._currentScrollOffset < _scrollTo)
        {
            this._currentScrollOffset += step;
        }
        else
        {
            this._currentScrollOffset -= step;
        }
        
        SetThreadPos(this._currentScrollOffset);
        this._scrollFrom = this._currentScrollOffset;
    }

    private void SetThreadPos(Double y)
    {
        this._oldScrollY = this._scrollViewer.Offset.Y;

        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            if (this._scrollTo > this._scrollFrom)
            {
                this._scrollViewer.ScrollDirection = ScrollDirection.DOWN;
            }
            else if (this._scrollTo < this._scrollFrom)
            {
                this._scrollViewer.ScrollDirection = ScrollDirection.UP;
            }

            if (this.IsSynced)
            {
                this._scrollViewer.Offset = new Vector(0, this._currentScrollOffset);
            }

            if (y - this._startMargin < 10 || this._scrollViewer.Offset.Y - this._startMargin < 10)
            {
                this._gradientTop.Opacity = 0;
            }
            else
            {
                this._gradientTop.Opacity = 1;
            }

            if ((this._scrollViewer.Extent.Height - this._scrollViewer.Offset.Y) == 
                this._scrollViewer.LargeChange.Height)
            {
                this._gradientBottom.Opacity = 0;
            }
            else
            {
                this._gradientBottom.Opacity = 1;
            }

            if (DataValidator.ValidateData(this._card))
            {
                if (DataValidator.ValidateData(this._card.LyricPart))
                {
                    if (this._card.LyricPart.Equals(this._lyricPart))
                    {
                        this._card.Percentage = (double)_viewModel.Percentage;
                    }
                }
            }
        }).GetAwaiter().GetResult();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void SetCurrentPosition(int selectedLine)
    {
        double position = 0;

        for (int i = 0; i < this._lyricParts.Count; i++)
        {
            var child = this._itemsRepeater.TryGetElement(i);
            
            if (i == selectedLine)
            {
                if (child is LyricsCard)
                {
                    this._card = (LyricsCard)child;
                }
                
                break;
            }
            else
            {

                if (child is LyricsCard)
                {
                    LyricsCard card = (LyricsCard)child;
                    card.Current = false;
                    card.Percentage = -10;
                }
                
                position += GetRenderedSize(i).Height;
            }
        }

        this._startMargin = CalcStartMargin();

        this._scrollFrom = this._scrollTo;
        
        this._scrollTo = CalcOffsetInViewPoint(selectedLine, position, this._startMargin);

        this._itemsRepeater.Margin = new Thickness(0,this._startMargin,0,0);
    }

    private double CalcStartMargin()
    {
        if (this._lyricParts.IsNullOrEmpty())
            return 0;
        
        double untilPos = this._scrollViewer.Viewport.Height / 2;
        untilPos -= GetRenderedSize(0).Height / 2;

        return untilPos;
    }

    private double CalcOffsetInViewPoint(int index, double currentSize, double startMargin)
    {
        double startAt = 0;
        
        for (int i = 0; i < index; i++)
        {
            startAt += GetRenderedSize(i).Height;
        }
        
        double untilPos = this._scrollViewer.Viewport.Height / 2;

        untilPos -= GetRenderedSize(index).Height / 2;
        
        /*double x = 0;
        int copyOfIndex = index - 1;

        while (x + GetRenderedSize(copyOfIndex).Height < untilPos)
        {
            x += GetRenderedSize(copyOfIndex).Height;
            copyOfIndex--;
        }

        if (x > untilPos)
            x = untilPos;*/
        
        return startAt - untilPos + startMargin;
    }
    
    public double CalcSpeed()
    {
        if (this._lyricParts.IsNullOrEmpty())
            return 15;

        LyricPart lastPart = null;
        double sum = 0;
        
        for (int i = 0; i < this._lyricParts.Count; i++)
        {
            LyricPart currentPart = this._lyricParts[i];
            
            if (lastPart == null)
            {
                lastPart = currentPart;
                continue;
            }
            else
            {
                sum += (currentPart.Time - lastPart.Time);
                lastPart = currentPart;
                continue;
            }
        }

        return (sum / this._lyricParts.Count) * 0.005f;
    }

    private Size GetRenderedSize(int index)
    {
        if (index < 0)
            return new Size(0, 0);
        
        if (this.FontWeight <= 0)
            return new Size(0, 0);
        
        if (this.LyricsFontSize <= 0)
            return new Size(0, 0);
        
        FormattedText text = new FormattedText(this._lyricParts[index].Part,
            new Typeface(FontFamily.Parse(
                "avares://Material.Styles/Fonts/Roboto#Roboto"), 
                FontStyle.Normal, this.LyricsFontWeight), this.LyricsFontSize, TextAlignment.Left,
            TextWrapping.Wrap, new Size(this.DesiredSize.Width - 5, this.DesiredSize.Height));

        double lineSize = 0;
        
        foreach (FormattedTextLine line in text.GetLines())
        {
            lineSize += line.Height;
        }

        Size returnVal = new Size(text.Bounds.Width, Math.Round(lineSize + this.ItemMargin.Bottom + 5));
        return returnVal;
    }

    public void Reset([CallerMemberName] string memberName = "")
    {
        this._scrollFrom = 0;
        this._currentScrollOffset = 0;
        this._scrollTo = 0;
        this._isFirstSync = true;
        
        if (DataValidator.ValidateData(this._itemsRepeater) &&
            DataValidator.ValidateData(this._itemsRepeater.Children))
        {
            this._itemsRepeater.Children.Clear();
        }
        
        if (DataValidator.ValidateData(this._scrollViewer))
        {
            this._scrollViewer.Offset = new Vector(0, 0);
        }
        
        ResyncOffset();
    }
    
    public void Reload()
    {
        this._itemsRepeater.Children.Clear();

        if (this.DataContext is LyricsScrollerViewModel)
        {
            LyricsScrollerViewModel context = (LyricsScrollerViewModel)this.DataContext;
            this._itemsRepeater.Children.Clear();
            
            SetAndRaise(LyricPartsProperty, ref _lyricParts,  null); 
            SetAndRaise(LyricPartsProperty, ref _lyricParts,  context.CurrentLyricParts);
        }
    }
    
    public int SelectedLine
    {
        get { return GetValue(SelectedLineProperty); }
        set
        {
            SetValue(SelectedLineProperty, value);
            SetCurrentPosition(value);
        }
    }
    
    public bool IsSynced
    {
        get { return GetValue(IsSyncedProperty); }
        set
        {
            SetValue(IsSyncedProperty, value);
        }
    }

    public ObservableCollection<LyricPart> LyricParts
    {
        get { return _lyricParts; }
        set
        {
            SetAndRaise(LyricPartsProperty, ref _lyricParts, value); 
            Reset();
            
            this._scrollSpeed = CalcSpeed();
        }
    }

    public LyricPart LyricPart
    {
        get
        {
            return _lyricPart;
        }
        set
        {
            if (!DataValidator.ValidateData(this._lyricParts))
                return;
            
            if (this._lyricParts.Count == 0)
                return;
            
            for (int i = 0; i < this._lyricParts.Count; i++)
            {
                if (this._lyricParts[i].Equals(value))
                {
                    SelectedLine = i;
                    this._lyricPart = value;
                    SetAndRaise(LyricPartProperty, ref _lyricPart, value);
                }
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

    public Thickness ItemMargin
    {
        get { return GetValue(ItemMarginProperty); }
        set { SetValue(ItemMarginProperty, value); }
    }
    
    public FontWeight LyricsFontWeight
    {
        get { return GetValue(LyricsFontWeightProperty); }
        set { SetValue(LyricsFontWeightProperty, value); }
    }
    
    public int LyricsFontSize
    {
        get { return GetValue(LyricsFontSizeProperty); }
        set { SetValue(LyricsFontSizeProperty, value); }
    }

    private void CTRL_Viewer_OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
    }


    private Vector _correctOffset = new Vector();
    private void CTRL_Viewer_OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        /*if (Math.Abs(this._scrollViewer.Offset.Y - _correctOffset.Y) > 200)
        {
            this._scrollViewer.Offset = _correctOffset;
        }
        
        _correctOffset = _scrollViewer.Offset;*/
        /*double diff = Math.Ceiling(Math.Abs(this._scrollViewer.Offset.Y - this._oldScrollY));
        double delta = Math.Abs(e.OffsetDelta.Y);
        
        if (diff < delta && e.OffsetDelta.Y % 5 == 0)
            this._scrollCount++;

        if (this._scrollCount >= 0)
            this.IsSynced = false;*/
    }

    public void ResyncOffset()
    {
        this._scrollCount = -2;
        this.IsSynced = true;
    }

    private void Layoutable_OnEffectiveViewportChanged(object? sender, EffectiveViewportChangedEventArgs e)
    {
        this.InvalidateVisual();
    }
}