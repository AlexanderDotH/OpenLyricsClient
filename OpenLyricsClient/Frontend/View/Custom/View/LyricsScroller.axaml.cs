using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.DesignerSupport;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Media;
using Avalonia.Rendering;
using Avalonia.Threading;
using Avalonia.Utilities;
using DevBase.Async.Task;
using DynamicData;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenLyricsClient.Backend;
using OpenLyricsClient.Backend.Events;
using OpenLyricsClient.Backend.Events.EventArgs;
using OpenLyricsClient.Backend.Helper;
using OpenLyricsClient.Backend.Structure.Enum;
using OpenLyricsClient.Backend.Structure.Lyrics;
using OpenLyricsClient.Backend.Structure.Song;
using OpenLyricsClient.Backend.Utils;
using OpenLyricsClient.Frontend.Controls.Model;
using SharpDX.DirectInput;
using Squalr.Engine.Utils.Extensions;
using TextAlignment = Avalonia.Media.TextAlignment;

namespace OpenLyricsClient.Frontend.View.Custom.View;

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

    private ObservableCollection<LyricPart> _lyricParts;
    private LyricPart _lyricPart;
    private LyricsCard _card;
    
    private double _scrollFrom;
    private double _currentScrollOffset;
    private double _scrollTo;

    private bool _isFirstSync;
    private bool _isInSycedMode;
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
        this._isInSycedMode = true;
        this._isFirstSync = true;
        this._scrollCount = -2;

        this._renderTimer = new SleepLoopRenderTimer(500);
        this._renderTimer.Tick += RenderTimerOnTick;
    }

    private void RenderTimerOnTick(TimeSpan obj)
    {
        if (_isInSycedMode)
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
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (this._isInSycedMode)
                this._scrollViewer.Offset = new Vector(0, y);

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
                    this._card.Percentage = (double)context.Percentage;
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
                }
                
                break;
            }
            else
            {
                position += GetRenderedSize(i).Height;
                
                if (child is LyricsCard)
                {
                    LyricsCard card = (LyricsCard)child;
                    card.Percentage = -1;
                }
            }
        }

        this._scrollFrom = this._scrollTo;
        this._scrollTo = CalcOffsetInViewPoint(selectedLine, position);
    }

    private double CalcOffsetInViewPoint(int index, double currentSize)
    {
        double viewPortHeight = this._scrollViewer.Viewport.Height / 2;

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

    public bool IsInSycedMode
    {
        get => _isInSycedMode;
        set => _isInSycedMode = value;
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
        double diff = Math.Ceiling(Math.Abs(this._currentScrollOffset - this._scrollTo));
        double delta = e.OffsetDelta.Y * 0.8;
        
        if (delta > diff)
            this._scrollCount++;

        if (this._scrollCount >= 0)
            this._isInSycedMode = false;
    }

    public void ResyncOffset()
    {
        this._scrollCount = -2;
        this._isInSycedMode = true;
    }
}

public class LyricsScrollerViewModel : INotifyPropertyChanged
{
    private TaskSuspensionToken _displayLyricsSuspensionToken;
    private TaskSuspensionToken _syncLyricsSuspensionToken;
    private TaskSuspensionToken _syncLyricsPercentageSuspensionToken;

    public ObservableCollection<LyricPart> _lyricParts;
    private LyricPart _lyricPart;
    private double _percentage;

    private RomanizationHelper _romanizationHelper;

    public LyricsScrollerViewModel()
    {
        this._romanizationHelper = new RomanizationHelper();
        
        if (!AvaloniaUtils.IsInPreviewerMode())
        {
            
            this._lyricParts = new ObservableCollection<LyricPart>();

            Core.INSTANCE.SongHandler.SongChanged += SongHandlerOnSongChanged;

            Core.INSTANCE.TaskRegister.Register(
                out _displayLyricsSuspensionToken,
                new Task(async () => await DisplayLyricsTask(), Core.INSTANCE.CancellationTokenSource.Token,
                    TaskCreationOptions.LongRunning),
                EnumRegisterTypes.SHOW_LYRICS);

            Core.INSTANCE.TaskRegister.Register(
                out _syncLyricsSuspensionToken,
                new Task(async () => await SyncLyricsTask(), Core.INSTANCE.CancellationTokenSource.Token,
                    TaskCreationOptions.LongRunning),
                EnumRegisterTypes.SYNC_LYRICS);
            
            Core.INSTANCE.TaskRegister.Register(
                out _syncLyricsPercentageSuspensionToken,
                new Task(async () => await SyncLyricsPercentageTask(), Core.INSTANCE.CancellationTokenSource.Token,
                    TaskCreationOptions.LongRunning),
                EnumRegisterTypes.SYNC_LYRICS_PERCENTAGE);
        }
    }

    private async Task DisplayLyricsTask()
    {
        while (!Core.IsDisposed())
        {
            await Task.Delay(1);
            await this._displayLyricsSuspensionToken.WaitForRelease();

            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                Song currentSong = Core.INSTANCE.SongHandler.CurrentSong;

                if (!DataValidator.ValidateData(currentSong))
                {
                    this.CurrentLyricParts =  new ObservableCollection<LyricPart>();
                    return;
                }

                if (!DataValidator.ValidateData(currentSong.Lyrics))
                {
                    this.CurrentLyricParts =  new ObservableCollection<LyricPart>();
                    return;
                }

                if (!DataValidator.ValidateData(currentSong.Lyrics.LyricParts))
                {
                    this.CurrentLyricParts =  new ObservableCollection<LyricPart>();
                    return;
                }

                if (!AreListsEqual(this.CurrentLyricParts, currentSong.Lyrics.LyricParts))
                {
                    this.CurrentLyricParts =  new ObservableCollection<LyricPart>(
                        await this._romanizationHelper.RomanizeArray(currentSong.Lyrics.LyricParts));
                }
            });
        }
    }

    private async Task SyncLyricsTask()
    {
        while (!Core.IsDisposed())
        {
            await Task.Delay(1);
            await this._syncLyricsSuspensionToken.WaitForRelease();

            Song currentSong = Core.INSTANCE.SongHandler.CurrentSong;

            if (!DataValidator.ValidateData(currentSong))
                continue;

            if (!DataValidator.ValidateData(currentSong.Lyrics))
                continue;

            if (!DataValidator.ValidateData(currentSong.CurrentLyricPart))
                continue;

            this.CurrentLyricPart = currentSong.CurrentLyricPart;
            this.CurrentLyricPart.Part = await this._romanizationHelper.RomanizeString(this.CurrentLyricPart.Part);
        }
    }
    
    private async Task SyncLyricsPercentageTask()
    {
        while (!Core.IsDisposed())
        {
            await Task.Delay(1);
            await this._syncLyricsPercentageSuspensionToken.WaitForRelease();

            Song currentSong = Core.INSTANCE.SongHandler.CurrentSong;

            if (!DataValidator.ValidateData(currentSong))
                continue;

            if (!DataValidator.ValidateData(currentSong.Lyrics))
                continue;

            if (!DataValidator.ValidateData(currentSong.CurrentLyricPart))
                continue;

            if (!DataValidator.ValidateData(this._lyricParts))
                continue;
            
            for (var i = 0; i < this._lyricParts.Count; i++)
            {
                if (this._lyricParts[i] == currentSong.CurrentLyricPart)
                {
                    if (i + 1 < this._lyricParts.Count)
                    {
                        LyricPart nextPart = this._lyricParts[i + 1];
                        
                        long time = nextPart.Time - currentSong.CurrentLyricPart.Time;
                        long currentTime = currentSong.Time - currentSong.CurrentLyricPart.Time;
                        double change = Math.Round((double)(100 * currentTime) / time);
                        
                        Percentage = change;
                    }
                    else
                    {
                        long time = currentSong.SongMetadata.MaxTime - currentSong.CurrentLyricPart.Time;
                        long currentTime = currentSong.Time - currentSong.CurrentLyricPart.Time;
                        double change = Math.Round((double)(100 * currentTime) / time);
                        
                        Percentage = change;
                    }
                }
            }
            
        }
    }
    
    private bool AreListsEqual(ObservableCollection<LyricPart> lyricPartsList1, LyricPart[] lyricPartsList2)
    {
        if (!DataValidator.ValidateData(lyricPartsList1) || !DataValidator.ValidateData(lyricPartsList2))
            return false;
        
        if (lyricPartsList2.Length != lyricPartsList1.Count)
            return false;
        
        for (int i = 0; i < lyricPartsList1.Count; i++)
        {
            LyricPart currentPart1 = lyricPartsList1[i];

            for (int j = 0; j < lyricPartsList2.Length; j++)
            {
                LyricPart currentPart2 = lyricPartsList2[i];
                if (!currentPart1.Part.Equals(currentPart2.Part))
                    return false;
            }
        }

        return true;
    }

    private void SongHandlerOnSongChanged(object sender, SongChangedEventArgs songchangedevent)
    {
        if (songchangedevent.EventType != EventType.POST)
            return;
    }

    public ObservableCollection<LyricPart> CurrentLyricParts
    {
        get => _lyricParts;
        set
        {
            _lyricParts = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CurrentLyricParts"));
        }
    }
    
    public LyricPart CurrentLyricPart
    {
        get => _lyricPart;
        set
        {
            _lyricPart = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CurrentLyricPart"));
        }
    }
    
    public double Percentage
    {
        get => _percentage;
        set
        {
            _percentage = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Percentage"));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

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
}