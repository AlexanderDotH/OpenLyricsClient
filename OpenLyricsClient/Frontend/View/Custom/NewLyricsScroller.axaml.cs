using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using DevBase.Async.Task;
using DynamicData;
using OpenLyricsClient.Backend;
using OpenLyricsClient.Backend.Events.EventArgs;
using OpenLyricsClient.Backend.Structure.Enum;
using OpenLyricsClient.Backend.Structure.Lyrics;
using OpenLyricsClient.Backend.Utils;
using OpenLyricsClient.Frontend.Animation;
using OpenLyricsClient.Frontend.Models.Custom;
using OpenLyricsClient.Frontend.Utils;
using Squalr.Engine.Utils.Extensions;

namespace OpenLyricsClient.Frontend.View.Custom;

public partial class NewLyricsScroller : UserControl
{
    // Controls
    private CustomScrollViewer _customScrollViewer;
    private ItemsRepeater _repeater;
    private ItemsRepeater _hiddenRepeater;

    // ViewModel
    private NewLyricsScrollerViewModel _viewModel;

    // Multithreadding
    private TaskSuspensionToken _suspensionToken;
    private SleepRenderTimer _renderTimer;
    
    private CancellationTokenSource _renderTimerCancellationTokenSource;
    
    // Variables
    private double _previousScrollOffset;
    private double _currentScrollOffset;
    private double _nextScrollOffset;
    private double _frameRate;
    private double _speed;
    
    public NewLyricsScroller()
    {
        InitializeComponent();

        this._previousScrollOffset = 0;
        this._currentScrollOffset = 0;
        this._nextScrollOffset = 0;
        this._speed = 2.0;

        this._frameRate = 144;
        
        this._viewModel = this.DataContext as NewLyricsScrollerViewModel;
        
        this._hiddenRepeater = this.Get<ItemsRepeater>(nameof(HIDDEN_CTRL_Repeater));
        this._repeater = this.Get<ItemsRepeater>(nameof(CTRL_Repeater));

        this._customScrollViewer = this.Get<CustomScrollViewer>(nameof(CTRL_Viewer));

        AttachedToVisualTree += OnAttachedToVisualTree;
    }

    private void SongHandlerOnSongChanged(object sender, SongChangedEventArgs songchangedevent)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            this._repeater.Margin = new Thickness(0, 1000, 0, 0);
            this._repeater.Opacity = 0.0d;
            this._customScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
        });
    }

    private void LyricHandlerOnLyricsFound(object sender)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            this._repeater.Margin = GetMargin();
            this._repeater.Opacity = 1.0d;
            this._customScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
        });

        this._speed = CalcSpeed() * 0.5f;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        Core.INSTANCE.TaskRegister.Register(
            out _suspensionToken, 
            new Task(async () => await CalculateOffset(), Core.INSTANCE.CancellationTokenSource.Token, TaskCreationOptions.LongRunning), 
            EnumRegisterTypes.GLOBAL_TICK);
        
        Core.INSTANCE.TaskRegister.Register(
            out _suspensionToken, 
            new Task(async () => await RenderTimer(), Core.INSTANCE.CancellationTokenSource.Token, TaskCreationOptions.LongRunning), 
            EnumRegisterTypes.GLOBAL_TICK);
        
        Core.INSTANCE.LyricHandler.LyricsFound += LyricHandlerOnLyricsFound;
        Core.INSTANCE.SongHandler.SongChanged += SongHandlerOnSongChanged;
    }

    private async Task RenderTimer()
    {
        int targetFrameTime = (int)Math.Round(1000 / this._frameRate);

        while (!Core.IsDisposed())
        {
            long frameStart = Stopwatch.GetTimestamp();

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                double start = this._nextScrollOffset;
                double end = this._currentScrollOffset;

                double y = SmoothAnimator.CalculateStep(
                    start,
                    end,
                    this._currentScrollOffset,
                    this._speed);

                this._customScrollViewer.Offset = new Vector(0, y);
                this._currentScrollOffset = y;
            });

            long frameEnd = Stopwatch.GetTimestamp();
            double elapsedMilliseconds = (frameEnd - frameStart) * 1000.0 / Stopwatch.Frequency;
            int delay = Math.Clamp(targetFrameTime - (int)elapsedMilliseconds, 10, 100);

            await Task.Delay(delay, Core.INSTANCE.CancellationTokenSource.Token);
        }
    }

    private async Task CalculateOffset()
    {
        var debounced = Debouncer.Debounce<LyricPart[], double>(
            async lyrics => 
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    return GetRenderedOffset(this._viewModel?.Lyric!, lyrics);
                }), 50);
        
        while (!Core.IsDisposed())
        {
            await Task.Delay(100);
            
            if (!DataValidator.ValidateData(this._viewModel?.Lyric, this._viewModel.Lyrics))
                continue;

            this._previousScrollOffset = this._nextScrollOffset;
            
            double offset = await debounced(this._viewModel?.Lyrics!);
            this._nextScrollOffset = offset;
        }
    }

    private double GetRenderedOffset(LyricPart lyricPart, LyricPart[] lyricParts)
    {
        if (!DataValidator.ValidateData(lyricPart, lyricParts))
            return 0;

        Thickness margin = GetMargin();
        
        int index = GetIndexOfLyric(lyricPart, lyricParts);

        double position = 0;

        for (int i = 0; i < index; i++)
            position += GetRenderedSize(i).Height;

        double halfHeight = this._customScrollViewer.Bounds.Height / 2;
        double halfCurrentLineHeight = GetRenderedSize(index).Height / 2;

        double item = (halfHeight - halfCurrentLineHeight);
        
        position -= item;
        position += margin.Top;

        return position;
    }

    private int GetIndexOfLyric(LyricPart lyricPart, LyricPart[] lyricParts) => lyricParts.IndexOf(lyricPart);

    private Size GetRenderedSize(int index)
    {
        if (!DataValidator.ValidateData(this._viewModel.Lyrics))
            return new Size();

        if (index > this._viewModel.Lyrics.Length || index < 0)
            return new Size();

        try
        {
            IControl realItem = this._repeater.TryGetElement(index);

            IControl itemContainer = this._hiddenRepeater.TryGetElement(index);

            if (itemContainer == null)
                itemContainer = this._hiddenRepeater.GetOrCreateElement(index);

            Size constraint = new Size(this.Bounds.Width, this.Bounds.Height);
            itemContainer.Measure(constraint);

            Size s = itemContainer.DesiredSize;

            realItem?.InvalidateVisual();

            return s;
        }
        catch (Exception e)
        {
            return new Size();
        }
    }

    public Thickness GetMargin()
    {
        if (!DataValidator.ValidateData(this._viewModel.Lyrics))
            return new Thickness();
        
        double m = this._customScrollViewer.Bounds.Height / 2;
        
        double up = GetRenderedSize(0).Height / 2;
        double down = GetRenderedSize(this._viewModel.Lyrics.Length - 1).Height / 2;

        return new Thickness(0, m + up, 0, m + down);
    }
    
    public float CalcSpeed()
    {
        if (this._viewModel.Lyrics.IsNullOrEmpty())
            return 15;

        LyricPart lastPart = null;
        float sum = 0;
        
        float highest = 0;
        int hSum = 0;
        
        for (int i = 0; i < this._viewModel.Lyrics.Length; i++)
        {
            LyricPart currentPart = this._viewModel.Lyrics[i];
            
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

        float speed = (sum / this._viewModel.Lyrics.Length);

        float hSA = highest / hSum;

        hSA *= 1.1f;
        hSA *= 1.1f;
        
        float percentage = 100 / hSA * speed;
        
        return 100.0F - percentage;
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        if (e.Delta.Y != 0)
        {
            //this.IsSynced = false;
        }

        if (e.Delta.Y > 0)
        {
            this._customScrollViewer.ScrollDirection = ScrollDirection.UP;
            this._customScrollViewer.Offset = new Vector(0, 
                this._customScrollViewer.Offset.Y - 
                this._customScrollViewer.SmallChange.Height * 2);
        }
        
        if (e.Delta.Y < 0)
        {
            this._customScrollViewer.ScrollDirection = ScrollDirection.DOWN;
            this._customScrollViewer.Offset = new Vector(0, 
                this._customScrollViewer.Offset.Y + 
                this._customScrollViewer.SmallChange.Height * 2);
        }
        
        base.OnPointerWheelChanged(e);
    }
}