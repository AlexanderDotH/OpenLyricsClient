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
using DevBase.Generics;
using OpenLyricsClient.Backend;
using OpenLyricsClient.Backend.Events.EventArgs;
using OpenLyricsClient.Backend.Events.EventHandler;
using OpenLyricsClient.Backend.Settings.Sections.Lyrics;
using OpenLyricsClient.Backend.Structure.Enum;
using OpenLyricsClient.Backend.Structure.Lyrics;
using OpenLyricsClient.Backend.Utils;
using OpenLyricsClient.Frontend.Animation;
using OpenLyricsClient.Frontend.Models.Custom;
using OpenLyricsClient.Frontend.Models.Elements;
using OpenLyricsClient.Frontend.Utils;
using OpenLyricsClient.Frontend.View.Windows;
using Squalr.Engine.Utils.Extensions;
using ScrollChangedEventArgs = OpenLyricsClient.Frontend.Models.Custom.ScrollChangedEventArgs;
using TextAlignment = Avalonia.Media.TextAlignment;

namespace OpenLyricsClient.Frontend.View.Custom;

public partial class LyricsScroller : UserControl
{
    public static LyricsScroller INSTANCE;
    
    public static readonly StyledProperty<int> SelectedLineProperty =
        AvaloniaProperty.Register<LyricsScroller, int>(nameof(SelectedLine));

    public static readonly StyledProperty<SolidColorBrush> SelectedLineBrushProperty =
        AvaloniaProperty.Register<LyricsScroller, SolidColorBrush>(nameof(SelectedLineBrush));

    public static readonly StyledProperty<SolidColorBrush> UnSelectedLineBrushProperty =
        AvaloniaProperty.Register<LyricsScroller, SolidColorBrush>(nameof(UnSelectedLineBrush));
    
    public static readonly StyledProperty<Thickness> ItemMarginProperty =
        AvaloniaProperty.Register<LyricsScroller, Thickness>(nameof(ItemMargin));
    
    public static readonly DirectProperty<LyricsScroller, LyricPart> LyricPartProperty = 
        AvaloniaProperty.RegisterDirect<LyricsScroller, LyricPart>(nameof(LyricPart), o => o.LyricPart, (o, v) => o.LyricPart = v);
    
    public static readonly DirectProperty<LyricsScroller, ObservableCollection<LyricPart>> LyricPartsProperty = 
        AvaloniaProperty.RegisterDirect<LyricsScroller, ObservableCollection<LyricPart>>(nameof(LyricParts), o => o.LyricParts, (o, v) => o.LyricParts = v);

    public static readonly StyledProperty<FontWeight> LyricsFontWeightProperty =
        AvaloniaProperty.Register<LyricsScroller, FontWeight>(nameof(LyricsFontWeight));

    public static readonly StyledProperty<bool> IsSyncedProperty =
        AvaloniaProperty.Register<LyricsScroller, bool>(nameof(IsSynced));

    private ObservableCollection<LyricPart> _lyricParts;
    private LyricPart _lyricPart;
    
    private LyricsCard _currentCard;
    private ATupleList<LyricsCard, bool> _lyricsRoll;

    private int _blurItemCount;
    private float _blurIncrement;
    
    private float _scrollFrom;
    private float _currentScrollOffset;
    private float _scrollTo;
    private float _oldScrollY;
    private int _oldIndex;

    private bool _isResynced;
    private int _scrollCount;

    private float _startMargin;
    private float _scrollSpeed;
    
    private CustomScrollViewer _scrollViewer;
    private ItemsRepeater _itemsRepeater;
    private ItemsRepeater _hiddenRepeater;

    private Grid _gradientTop;
    private Grid _gradientBottom;
    
    private SleepRenderTimer _renderTimer;
    private UiThreadRenderTimer _uiThreadRenderTimer;

    private LyricsScrollerViewModel _viewModel;
    public event BlurChangedEventHandler BlurChanged;

    public LyricsScroller()
    {
        INSTANCE = this;

        InitializeComponent();

        this._viewModel = new LyricsScrollerViewModel();
        this.DataContext = this._viewModel;

        this._scrollViewer = this.Get<CustomScrollViewer>(nameof(CTRL_Viewer));
        this._itemsRepeater = this.Get<ItemsRepeater>(nameof(CTRL_Repeater));
        this._hiddenRepeater = this.Get<ItemsRepeater>(nameof(HIDDEN_CTRL_Repeater));
            
        this._gradientTop = this.Get<Grid>(nameof(GradientTop));
        this._gradientBottom = this.Get<Grid>(nameof(GradientBottom));

        this._scrollFrom = 0;
        this._currentScrollOffset = 0;
        this._scrollTo = 0;
        this.IsSynced = true;
        this._isResynced = true;
        this._scrollCount = -2;
        this._oldScrollY = 0;
        this._startMargin = 0;
        this._scrollSpeed = 15;
        this._oldIndex = 0;

        this._blurIncrement = 0.8F;
        this._blurItemCount = 6;
        this._lyricsRoll = new ATupleList<LyricsCard, bool>();
        
        this._renderTimer = new SleepRenderTimer(150);
        this._renderTimer.Tick += RenderTimerOnTick;
        
        this._uiThreadRenderTimer = new UiThreadRenderTimer(150);
        this._uiThreadRenderTimer.Tick += UiThreadRenderTimerOnTick;
    }

    private void UiThreadRenderTimerOnTick(TimeSpan obj)
    {
        this._startMargin = CalcStartMargin();
        
        this._itemsRepeater.Margin = new Thickness(0,this._startMargin,0,0);
        
        this._gradientTop.IsVisible = !this._lyricParts.IsNullOrEmpty();
        this._gradientBottom.IsVisible = !this._lyricParts.IsNullOrEmpty();
        
        if (this._scrollViewer.Offset.Y - this._startMargin < 10)
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
    }

    private void RenderTimerOnTick(TimeSpan obj)
    {
        if (!this._isResynced)
        {
            float step = Math.Abs(this._scrollTo - this._currentScrollOffset) / (this._scrollSpeed * 0.3f);
        
            if (this._currentScrollOffset < _scrollTo)
            {
                this._currentScrollOffset += step;
            }
            else
            {
                this._currentScrollOffset -= step;
            }
        
            float diff = Math.Abs(this._currentScrollOffset - (float)this._scrollViewer.Offset.Y);
        
            if (diff < 1)
            {
                this._isResynced = true;
            }
        }
        else
        {
            float start = this._scrollTo;
            float end = this._oldScrollY;

            float y = (float)SmoothAnimator.CalculateStep(
                start, 
                end, 
                this._currentScrollOffset, 
                this._scrollSpeed);
            
            this._currentScrollOffset = y;
        }
        
        if (!double.IsNaN(this._currentScrollOffset))
            SetThreadPos(this._currentScrollOffset);
    }

    private void SetThreadPos(Double y)
    {
        this._oldScrollY = (float)this._scrollViewer.Offset.Y;

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
                this._scrollViewer.Offset = new Vector(0, y);
            }

            if (DataValidator.ValidateData(this._currentCard))
            {
                if (DataValidator.ValidateData(this._currentCard.LyricPart))
                {
                    if (this._currentCard.LyricPart.Equals(this._lyricPart))
                    {
                        this._currentCard.Percentage = (double)_viewModel.Percentage;
                    }
                }
            }
        }).GetAwaiter().GetResult();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void TryBlurItem(int index, float blurSigma)
    {
        if (index < 0)
            return;
        
        if (index > this._lyricParts?.Count - 1)
            return;
        
        BlurChangedEvent(new BlurChangedEventArgs(blurSigma, this._lyricParts![index]));

        try
        {
            IControl item = this._itemsRepeater.TryGetElement(index);
        
            if (item is LyricsCard)
            {
                LyricsCard cItem = (LyricsCard)item;
                cItem.BlurSigma = blurSigma;
                cItem.InvalidateVisual();
            }
        }
        catch (Exception e)
        {
        }
    }
    
    private void SetCurrentPosition(int selectedLine)
    {
        //this._lyricsRoll.Clear();
        
        float currentSize = this._blurIncrement;
        
        for (int i = 0; i < this._lyricParts.Count; i++)
        {
            var child = this._itemsRepeater.TryGetElement(i);
            
            /*if (child is LyricsCard)
            {
                if ((i != selectedLine && i < selectedLine - this._blurItemCount) || 
                    (i != selectedLine && i > selectedLine - this._blurItemCount))
                {
                    this._lyricsRoll.Add((LyricsCard)child, false);
                }
            }*/
            
            bool useBlur = Core.INSTANCE.SettingsHandler.Settings<LyricsSection>()!.GetValue<bool>("Blur Lyrics");
            
            if (i == selectedLine)
            {
                if (useBlur)
                {
                    for (int j = 1; j < this._blurItemCount; j++)
                    {
                        TryBlurItem(i - j, IsSynced && this._isResynced ? currentSize : 0);
                        TryBlurItem(i + j, IsSynced && this._isResynced ? currentSize : 0);
                        currentSize += this._blurIncrement;
                    }
                }
                
                if (child is LyricsCard)
                {
                    this._currentCard = (LyricsCard)child;
                    TryBlurItem(i, 0);
                }
                
                break;
            }
            else
            {
                if (!this.IsSynced || !useBlur)
                {
                    TryBlurItem(i, 0);
                }
                
                if (child is LyricsCard)
                {
                    LyricsCard card = (LyricsCard)child;
                    card.Current = false;
                    card.Percentage = -10;
                }
            }
        }

        /*float currentSize = this._blurIncrement;
        
        for (int i = 0; i < this._lyricsRoll.Length; i++)
        {
            if (!DataValidator.ValidateData(this._lyricsRoll.Get(i)))
                continue;
            
            LyricsCard card = this._lyricsRoll.Get(i).Item1;

            if (card.Equals(this._currentCard))
            {
                for (int j = 0; j < this._blurItemCount; j++)
                {
                    int nextPos = i + j;
                    
                    if (nextPos < i)
                    {
                        this._lyricsRoll.Get(nextPos).Item1.BlurSigma = currentSize;
                        currentSize += _blurIncrement;
                    }
                }
            }
        }*/

        this._scrollFrom = this._scrollTo;

        if (this._isResynced)
        {
            this._scrollTo = CalcOffsetInViewPoint(selectedLine, this._startMargin);
        }

        this._itemsRepeater.Margin = new Thickness(0,this._startMargin,0,0);
    }

    private float CalcStartMargin()
    {
        if (this._lyricParts.IsNullOrEmpty())
            return 0;
        
        float untilPos = (float)this._scrollViewer.Viewport.Height / 2.25F;
        
        //untilPos -= (float)GetRenderedSize(0).Height / 2.0F;

        return untilPos;
    }

    private float CalcOffsetInViewPoint(int index, float startMargin)
    {
        float startAt = 0;
        
        for (int i = 0; i < index; i++)
        {
            startAt += (float)GetRenderedSize(i).Height;
        }
        
        float untilPos = (float)this._scrollViewer.Viewport.Height / 2.25F;

        //untilPos -= (float)GetRenderedSize(index).Height / 2.0F;

        return startAt - untilPos + startMargin;
    }
    
    public float CalcSpeed()
    {
        if (this._lyricParts.IsNullOrEmpty())
            return 15;

        LyricPart lastPart = null;
        float sum = 0;
        
        float highest = 0;
        int hSum = 0;
        
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
                float value = (currentPart.Time - lastPart.Time);
                
                sum += value;

                if (value > highest)
                {
                    highest += value;
                    hSum++;
                }

                lastPart = currentPart;
                continue;
            }
        }

        float speed = (sum / this._lyricParts.Count);

        float hSA = highest / hSum;

        hSA *= 1.1f;
        hSA *= 1.1f;
        
        float percentage = 100 / hSA * speed;
        
        return 100.0F - percentage;
    }

    private Size GetRenderedSize(int index)
    {
        var realItem = this._itemsRepeater.TryGetElement(index);

        var itemContainer = this._hiddenRepeater.TryGetElement(index);

        if (itemContainer == null)
            itemContainer = this._hiddenRepeater.GetOrCreateElement(index);
        
        var constraint = new Size(this.DesiredSize.Width, this.DesiredSize.Height);
        itemContainer.Measure(constraint);

        Size s = itemContainer.DesiredSize;
        
        realItem?.InvalidateVisual();
        
        return s;
        
        /*if (index < 0)
            return new Size(0, 0);
        
        if (this.FontWeight <= 0)
            return new Size(0, 0);

        if (index > this._lyricParts.Count - 1)
            return new Size(0, 0);
            
        FormattedText text = new FormattedText(this._lyricParts[index].Part,
            new Typeface(FontFamily.Parse(
                "avares://Material.Styles/Fonts/Roboto#Roboto"), 
                FontStyle.Normal, this.LyricsFontWeight), this.FontSize, TextAlignment.Left,
            TextWrapping.Wrap, new Size(this.DesiredSize.Width - 5, this.DesiredSize.Height));

        double lineSize = 0;
        
        foreach (FormattedTextLine line in text.GetLines())
        {
            lineSize += line.Height;
        }

        Size returnVal = new Size(text.Bounds.Width, Math.Floor(lineSize + this.ItemMargin.Bottom + 5));
        return returnVal;*/
    }

    public void Reset([CallerMemberName] string memberName = "")
    {
        this._scrollFrom = 0;
        this._currentScrollOffset = 0;
        this._scrollTo = 0;
        this._isResynced = true;
        
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

            if (value != this._oldIndex)
            {
                SetCurrentPosition(value);
                this._oldIndex = value;
            }
            
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

    public SolidColorBrush SelectedLineBrush
    {
        get { return GetValue(SelectedLineBrushProperty); }
        set { SetValue(SelectedLineBrushProperty, value); }
    }
    
    public SolidColorBrush UnSelectedLineBrush
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

    public float BlurIncrement
    {
        get => this._blurIncrement;
        set => this._blurIncrement = value;
    }

    public int BlurItemCount
    {
        get => this._blurItemCount;
        set => this._blurItemCount = value;
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        if (e.Delta.Y != 0)
        {
            this.IsSynced = false;
        }

        if (e.Delta.Y > 0)
        {
            this._scrollViewer.ScrollDirection = ScrollDirection.UP;
            this._scrollViewer.Offset = new Vector(0, 
                this._scrollViewer.Offset.Y - 
                this._scrollViewer.SmallChange.Height * 2);
        }
        
        if (e.Delta.Y < 0)
        {
            this._scrollViewer.ScrollDirection = ScrollDirection.DOWN;
            this._scrollViewer.Offset = new Vector(0, 
                this._scrollViewer.Offset.Y + 
                this._scrollViewer.SmallChange.Height * 2);
        }
        base.OnPointerWheelChanged(e);
    }

    public void ResyncOffset()
    {
        this._scrollCount = -2;

        if (!DataValidator.ValidateData(this._scrollViewer))
            return;
        
        this._currentScrollOffset = (float)this._scrollViewer.Offset.Y;
        this._isResynced = false;
        this.IsSynced = true;
    }
    
    protected virtual void BlurChangedEvent(BlurChangedEventArgs blurChangedEventArgs)
    {
        BlurChangedEventHandler blurChangedEvent = BlurChanged;
        blurChangedEvent?.Invoke(this, blurChangedEventArgs);
    }

    private void Layoutable_OnEffectiveViewportChanged(object? sender, EffectiveViewportChangedEventArgs e)
    {
        this.InvalidateVisual();
    }
}