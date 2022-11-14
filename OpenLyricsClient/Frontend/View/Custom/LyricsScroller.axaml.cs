using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Rendering;
using Avalonia.Threading;
using DevBase.Async.Task;
using OpenLyricsClient.Backend;
using OpenLyricsClient.Backend.Events;
using OpenLyricsClient.Backend.Events.EventArgs;
using OpenLyricsClient.Backend.Helper;
using OpenLyricsClient.Backend.Structure.Enum;
using OpenLyricsClient.Backend.Structure.Lyrics;
using OpenLyricsClient.Backend.Structure.Song;
using OpenLyricsClient.Backend.Utils;
using OpenLyricsClient.Frontend.Models.Custom;
using OpenLyricsClient.Frontend.Models.Elements;
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
    
    private ScrollViewer _scrollViewer;
    private ItemsRepeater _itemsRepeater;

    private Grid _gradientTop;
    private Grid _gradientBottom;
    
    private SleepLoopRenderTimer _renderTimer;
    
    public LyricsScroller()
    {
        InitializeComponent();

        this.DataContext = new LyricsScrollerViewModel();

        this._scrollViewer = this.Get<ScrollViewer>(nameof(CTRL_Viewer));
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

        this._renderTimer = new SleepLoopRenderTimer(500);
        this._renderTimer.Tick += RenderTimerOnTick;

        /*Core.INSTANCE.SettingManager.SettingsChanged  += (sender, args) =>
        {
            Reload();
            Reset();
        };*/
    }

    private void RenderTimerOnTick(TimeSpan obj)
    {
        SetThreadPos(_currentScrollOffset);
        
        /*if (this._isFirstSync && this._lyricParts != null && this._lyricPart != null)
        {
            SetThreadPos(_scrollTo);
            this._currentScrollOffset = this._scrollTo;
            this._isFirstSync = false;
        }*/

        int divider = 15;
        
        if (_currentScrollOffset < _scrollTo)
        {
            double step = Math.Abs(_scrollFrom - _scrollTo) / divider;
            _currentScrollOffset += step;
            _scrollFrom = _currentScrollOffset;
        }
        else
        {
            double step = Math.Abs(_scrollFrom - _scrollTo) / divider;
            _currentScrollOffset -= step;
            _scrollFrom = _currentScrollOffset;
        }
    }

    private void SetThreadPos(Double y)
    {
        this._oldScrollY = this._scrollViewer.Offset.Y;

        Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (this.IsSynced)
            {
                this._oldScrollY = this._scrollViewer.Offset.Y;
                this._scrollViewer.Offset = new Vector(0, y);
            }
            
            if (y < 10)
            {
                this._gradientTop.Opacity = 0;
            }
            else
            {
                this._gradientTop.Opacity = 1;
            }

            if (DataValidator.ValidateData(this._card))
            {
                if (this.DataContext is LyricsScrollerViewModel)
                {
                    LyricsScrollerViewModel context = (LyricsScrollerViewModel)this.DataContext;

                    if (DataValidator.ValidateData(this._card.LyricPart))
                    {
                        if (this._card.LyricPart.Equals(this._lyricPart))
                        {
                            this._card.Percentage = (double)context.Percentage;
                        }
                    }
                }
            }
        });
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
                    //this._card.Focus();
                }
                
                break;
            }
            else
            {
                position += GetRenderedSize(i).Height;
                
                /*if (child is LyricsCard)
                {
                    LyricsCard card = (LyricsCard)child;
                    card.Percentage = -1;
                    card.IsEnabled = false;
                }*/
            }
        }

        //TryUnselectAll();
        
        this._scrollFrom = this._scrollTo;
        this._scrollTo = CalcOffsetInViewPoint(selectedLine, position);
    }

    private void TryUnselectAll()
    {
        for (int i = 0; i < this._lyricParts.Count; i++)
        {
            var child = this._itemsRepeater.TryGetElement(i);
            
            if (child is LyricsCard)
            {
                LyricsCard element = (LyricsCard)child;
                if (element != this._card)
                {
                    element.Percentage = -1;
                    element.IsEnabled = false;
                }
            }
        }
    }

    private double CalcOffsetInViewPoint(int index, double currentSize)
    {
        double viewPortHeight = this._scrollViewer.Viewport.Height / 1.16;

        double x = 0;
        int currentPos = index;

        double nextValue = 0;
        while ((nextValue + x + GetRenderedSize(currentPos).Height) < viewPortHeight)
        {
            nextValue = x + GetRenderedSize(currentPos).Height;
            x = nextValue;

            if (currentPos - 1 >= 0)
                currentPos--;
        }

        return currentSize - x;
    }

    private Size GetRenderedSize(int index)
    {
        if (index < 0)
            return new Size(0, 0);
        
        FormattedText text = new FormattedText(this._lyricParts[index].Part,
            new Typeface(FontFamily.Parse(
                "avares://Material.Styles/Fonts/Roboto#Roboto, Noto Sans, BlinkMacSystemFont, Segoe UI, Helvetica Neue, Helvetica, Cantarell, Ubuntu, Arial, Hiragino Kaku Gothic Pro, MS UI Gothic, MS PMincho, Microsoft JhengHei, Microsoft JhengHei UI, Microsoft YaHei New, Microsoft Yahei, SimHei"), 
                FontStyle.Normal, this.LyricsFontWeight), this.LyricsFontSize, TextAlignment.Left,
            TextWrapping.Wrap, new Size(this._itemsRepeater.DesiredSize.Width, 0));

        Size returnVal = new Size(text.Bounds.Width, Math.Floor(text.Bounds.Height + ItemMargin.Bottom + 5));
        return returnVal;
    }

    public void Reset()
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
                if (this._lyricParts[i] == value)
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
    
    private void CTRL_Viewer_OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        double diff = Math.Ceiling(Math.Abs(this._scrollViewer.Offset.Y - this._oldScrollY));
        double delta = Math.Abs(e.OffsetDelta.Y);
        
        if (e.OffsetDelta.Y % 50 == 0)
            this._scrollCount++;

        if (this._scrollCount >= 0)
            this.IsSynced = false;
    }

    public void ResyncOffset()
    {
        this._scrollCount = -2;
        this.IsSynced = true;
    }
}