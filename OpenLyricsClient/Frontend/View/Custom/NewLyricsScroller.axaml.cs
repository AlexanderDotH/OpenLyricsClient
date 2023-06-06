﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Rendering;
using Avalonia.Threading;
using DevBase.Async.Task;
using DynamicData;
using OpenLyricsClient.Backend;
using OpenLyricsClient.Backend.Events;
using OpenLyricsClient.Backend.Events.EventArgs;
using OpenLyricsClient.Shared.Structure.Enum;
using OpenLyricsClient.Shared.Structure.Lyrics;
using OpenLyricsClient.Shared.Utils;
using OpenLyricsClient.Frontend.Animation;
using OpenLyricsClient.Frontend.Models.Custom;
using OpenLyricsClient.Frontend.Structure.Enum;
using OpenLyricsClient.Frontend.Utils;
using OpenLyricsClient.Frontend.View.Custom.Tile;
using Squalr.Engine.Utils.Extensions;

namespace OpenLyricsClient.Frontend.View.Custom;

public partial class NewLyricsScroller : UserControl, INotifyPropertyChanged
{
    // Styled Properties
    public static readonly StyledProperty<bool> IsSyncedProperty =
        AvaloniaProperty.Register<LyricsScroller, bool>(nameof(IsSynced));

    // Controls
    private CustomScrollViewer _customScrollViewer;
    private ItemsRepeater _repeater;
    private ItemsRepeater _hiddenRepeater;
    private Panel _container;

    // ViewModel
    private NewLyricsScrollerViewModel _viewModel;
    
    // Instance
    private static NewLyricsScroller _instance;

    // Multithreadding
    private TaskSuspensionToken _suspensionToken;
    private UiThreadRenderTimer _uiThreadRenderTimer;

    private CancellationTokenSource _renderTimerCancellationTokenSource;
    
    // Variables
    private double _currentScrollOffset;
    private double _nextScrollOffset;
    private double _frameRate;
    private double _speed;
    private bool _isSyncing;
    
    public event PropertyChangedEventHandler? PropertyChanged;
    
    public NewLyricsScroller()
    {
        AvaloniaXamlLoader.Load(this);

        _instance = this;
        
        this._currentScrollOffset = 0;
        this._nextScrollOffset = 0;
        this.Speed = 15 * 0.1;

        this._isSyncing = false;

        this._frameRate = 144;

        Reset();
        
        this.DataContext = new NewLyricsScrollerViewModel();
        this._viewModel = this.DataContext as NewLyricsScrollerViewModel;
        
        this._hiddenRepeater = this.Get<ItemsRepeater>(nameof(HIDDEN_CTRL_Repeater));
        this._repeater = this.Get<ItemsRepeater>(nameof(CTRL_Repeater));
        this._customScrollViewer = this.Get<CustomScrollViewer>(nameof(CTRL_Viewer));
        this._container = this.Get<Panel>(nameof(CTRL_Container));
        
        this._uiThreadRenderTimer = new UiThreadRenderTimer(150);
        this._uiThreadRenderTimer.Tick += UiThreadRenderTimerOnTick;

        AttachedToVisualTree += OnAttachedToVisualTree;
    }

    private void UiThreadRenderTimerOnTick(TimeSpan obj)
    {
        this._repeater.Margin = GetMargin();

        if (DataValidator.ValidateData(this._viewModel.Lyrics))
            this._repeater.Opacity = 1.0d;

        double y = this._customScrollViewer.Offset.Y;
        
        if (this.IsSynced && !this._isSyncing)
        {
            y = SmoothAnimator.Lerp(
                this._currentScrollOffset,
                this._nextScrollOffset,
                (int)obj.Milliseconds, this.Speed, EnumAnimationStyle.CIRCULAREASEOUT);
        }
        else if (!this.IsSynced && this._isSyncing)
        {
            y = CalcResyncStep(this._currentScrollOffset, this._nextScrollOffset, this.Speed);
        }
        
        if (!double.IsNaN(y))
        {
            this._customScrollViewer.ScrollDirection = ScrollDirection.DOWN;
            this._customScrollViewer.Offset = new Vector(0, y);
            this._currentScrollOffset = y;
        }
    }

    private double CalcResyncStep(double currentOffset, double nextOffset, double speed)
    {
        double step = Math.Abs(nextOffset - currentOffset) / (speed);
        
        currentOffset += (currentOffset < nextOffset) ? step : -step;
        
        double diff = Math.Abs(nextOffset - currentOffset);

        if (diff < 1 && this._isSyncing)
        {
            this.IsSynced = true;
            this._isSyncing = false;
        }

        return currentOffset;
    }
    
    private void SongHandlerOnSongChanged(object sender, SongChangedEventArgs songchangedevent)
    {
        if (songchangedevent.EventType != EventType.PRE)
            return;
    
        Reset();
    }

    private void LyricHandlerOnLyricsFound(object sender, LyricsFoundEventArgs args)
    {
        //Reset();

        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            this._customScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
        });

        this.Speed = args.LyricData.LyricSpeed * 0.1f;
    }

    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        Core.INSTANCE.TaskRegister.Register(
            out _suspensionToken, 
            new Task(async () => await CalculateOffset(), Core.INSTANCE.CancellationTokenSource.Token, TaskCreationOptions.LongRunning), 
            EnumRegisterTypes.GLOBAL_TICK);
        
        Core.INSTANCE.LyricHandler.LyricsFound += LyricHandlerOnLyricsFound;
        Core.INSTANCE.SongHandler.SongChanged += SongHandlerOnSongChanged;
    }

    private async Task CalculateOffset()
    {
        while (!Core.IsDisposed())
        {
            await Task.Delay(100);
            
            if (!DataValidator.ValidateData(this._viewModel?.Lyric, this._viewModel.Lyrics))
                continue;

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                double offset = GetRenderedOffset(this._viewModel?.Lyric!, this._viewModel.Lyrics);
                this._nextScrollOffset = offset;
            });
        }
    }

    private double GetRenderedOffset(LyricPart lyricPart, ObservableCollection<LyricPart> lyricParts)
    {
        if (!DataValidator.ValidateData(lyricPart, lyricParts))
            return 0;

        Thickness margin = GetMargin();
        
        int index = GetIndexOfLyric(lyricPart, lyricParts);

        double position = 0;

        for (int i = 0; i < index; i++)
            position += GetRenderedSize(i).Height;

        double halfHeight = this._customScrollViewer.Viewport.Height / 2.2d;

        position -= halfHeight;
        position += margin.Top;

        return position;
    }

    private int GetIndexOfLyric(LyricPart lyricPart, ObservableCollection<LyricPart> lyricParts) => lyricParts.IndexOf(lyricPart);

    private Size GetRenderedSize(int index)
    {
        if (!DataValidator.ValidateData(this._viewModel.Lyrics))
            return new Size();

        if (index > this._viewModel.Lyrics.Count || index < 0)
            return new Size();

        try
        {
            LyricsTile itemContainer = this._hiddenRepeater.TryGetElement(index) as LyricsTile;

            if (itemContainer == null)
                itemContainer = this._hiddenRepeater.GetOrCreateElement(index) as LyricsTile;

            return itemContainer.Size;
        }
        catch (Exception e)
        {
            return new Size();
        }
    }

    public Thickness GetMargin()
    {
        double m = this._customScrollViewer.Viewport.Height / 2.2d;
        return new Thickness(0, m, 0, m);
    }
    
    private void Reset()
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            this._repeater.Opacity = 0;
            this._customScrollViewer.ScrollDirection = ScrollDirection.DOWN;
            this._currentScrollOffset = 0;
            this._customScrollViewer.Offset = new Vector(0, 0);
            this._customScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            
            //Resync();
            this.IsSynced = true;
        });
    }
    
    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        if (e.Delta.Y != 0)
        {
            this.IsSynced = false;
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


    public static NewLyricsScroller Instance
    {
        get => _instance;
    }
    
    public void Resync()
    {
        this._currentScrollOffset = this._customScrollViewer.Offset.Y;
        this._isSyncing = true;
    }

    public bool IsSynced
    {
        get => GetValue(IsSyncedProperty);
        set
        {
            SetValue(IsSyncedProperty, value);
            OnPropertyChanged("IsSynced");
        }
    }

    public double Speed
    {
        get => this._speed;
        set => SetField(ref this._speed, value);
    }
}