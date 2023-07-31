using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Accord.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Rendering;
using Avalonia.Threading;
using Avalonia.VisualTree;
using DevBase.Async.Task;
using DevBase.Generics;
using OpenLyricsClient.Logic;
using OpenLyricsClient.Logic.Debugger;
using OpenLyricsClient.Logic.Events;
using OpenLyricsClient.Logic.Events.EventArgs;
using OpenLyricsClient.Shared.Structure.Enum;
using OpenLyricsClient.Shared.Structure.Lyrics;
using OpenLyricsClient.Shared.Utils;
using OpenLyricsClient.UI.Animation;
using OpenLyricsClient.UI.Models.Custom;
using OpenLyricsClient.UI.Structure.Enum;
using OpenLyricsClient.UI.View.Custom.Tile;
using Debug = System.Diagnostics.Debug;

namespace OpenLyricsClient.UI.View.Custom;

public partial class LyricsScroller : UserControl, INotifyPropertyChanged
{
    // Styled Properties
    public static readonly StyledProperty<bool> IsSyncedProperty =
        AvaloniaProperty.Register<LyricsScroller, bool>(nameof(IsSynced));

    // Controls
    private ScrollViewer _scrollViewer;
    private ItemsRepeater _repeater;
    
    private ItemsRepeater _hiddenRepeater;
    private ItemsControl _hiddenItemsControl;
    
    // ViewModel
    private LyricsScrollerViewModel _viewModel;
    
    // Instance
    private static LyricsScroller _instance;

    // Multithreadding
    private TaskSuspensionToken _suspensionToken;
    private UiThreadRenderTimer _uiThreadRenderTimer;

    private CancellationTokenSource _renderTimerCancellationTokenSource;
    
    // Variables
    private double _speed;
    private bool _isSyncing;
    private bool _isResyncing;

    private LyricPart _targetLock;
    private LyricPart _lastPart;

    private Debugger<LyricsScroller> _debugger;

    private ATupleList<int, ScrollerElement> _measurementCache;
    
    public event PropertyChangedEventHandler? PropertyChanged;
    
    public LyricsScroller()
    {
        AvaloniaXamlLoader.Load(this);

        this._debugger = new Debugger<LyricsScroller>(this);

        this._measurementCache = new ATupleList<int, ScrollerElement>();
        
        _instance = this;

        this._isResyncing = false;
        
        this.Speed = 15 * 0.1;

        this._isSyncing = false;

        Reset();
        
        this.DataContext = new LyricsScrollerViewModel();
        this._viewModel = this.DataContext as LyricsScrollerViewModel;
        
        this._hiddenItemsControl = this.Get<ItemsControl>(nameof(HIDDEN_CTRL_ItemsControl));
        
        this._hiddenRepeater = this.Get<ItemsRepeater>(nameof(HIDDEN_CTRL_Repeater));
        //this._hiddenRepeater.Height = double.PositiveInfinity;
        
        this._repeater = this.Get<ItemsRepeater>(nameof(CTRL_Repeater));
        this._scrollViewer = this.Get<ScrollViewer>(nameof(CTRL_Viewer));
        
        this.EffectiveViewportChanged += OnEffectiveViewportChanged;
        
        /*this._uiThreadRenderTimer = new UiThreadRenderTimer(150);
        this._uiThreadRenderTimer.Tick += UiThreadRenderTimerOnTick;*/

        AttachedToVisualTree += OnAttachedToVisualTree;
    }

    private void OnEffectiveViewportChanged(object? sender, EffectiveViewportChangedEventArgs e)
    {
        this._measurementCache.Clear();
    }

    private void SongHandlerOnSongChanged(object sender, SongChangedEventArgs songchangedevent)
    {
        if (songchangedevent.EventType != EventType.PRE)
            return;
    
        Reset();
    }

    private void LyricHandlerOnLyricsFound(object sender, LyricsFoundEventArgs args)
    {
        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            this._scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
        });
        
        this._measurementCache.Clear();

        this.Speed = args.LyricData.LyricSpeed * 0.1f;
    }

    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        Core.INSTANCE.LyricHandler.LyricsFound += LyricHandlerOnLyricsFound;
        Core.INSTANCE.LyricHandler.LyricChanged += LyricHandlerOnLyricChanged;
        Core.INSTANCE.SongHandler.SongChanged += SongHandlerOnSongChanged;
    }
    
    private void LyricHandlerOnLyricChanged(object sender, LyricChangedEventArgs lyricchangedeventargs)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (lyricchangedeventargs.LyricPart.Equals(this._lastPart)) 
                return;

            this._hiddenItemsControl.IsVisible = true;
            this._hiddenItemsControl.Opacity = 0;
            
            double offset = GetRenderedOffset(lyricchangedeventargs.LyricPart, this._viewModel.Lyrics);
            this._repeater.Margin = GetMargin();
            this._scrollViewer.Offset = new Vector(0, offset);

            this._lastPart = lyricchangedeventargs.LyricPart;
        });
    }

    private double GetRenderedOffset(LyricPart lyricPart, ObservableCollection<LyricPart> lyricParts)
    {
        if (!DataValidator.ValidateData(lyricPart, lyricParts))
            return 0;

        Thickness margin = GetMargin();
       
        int index = GetIndexOfLyric(lyricPart, lyricParts);
        double position = 0;

        for (int i = 0; i < index; i++)
        {
            Size element = GetRenderedSize(i);
            
            position += element.Height;
        }

        double halfHeight = this._scrollViewer.Viewport.Height / 2.2d;
        
        position -= halfHeight;
        position += margin.Top;

        return position;
    }

    private Size GetRenderedSize(int index)
    {
        try
        {
            ScrollerElement e = this._measurementCache.FindEntry(index);

            if (DataValidator.ValidateData(e))
                return e.Size;
        }
        catch (Exception e)
        {
            this._debugger.Write(e);
        }
        
        Size repeater = RepeaterSize(index);
        Size container = ContainerSize(index);

        Size value = new Size(Math.Max(repeater.Width, container.Width), Math.Max(repeater.Height, container.Height));

        if (value.Height == 70)
            return new Size(0, 111);
        
        ScrollerElement element = new ScrollerElement()
        {
            Index = index,
            Size = value
        };
 
        this._measurementCache.Add(index, element);
        
        return value;
    }

    private Size ContainerSize(int index)
    {
        this._hiddenItemsControl.UpdateLayout();
        
        try
        {
            Control content = this._hiddenItemsControl.ContainerFromIndex(index);

            if (DataValidator.ValidateData(content) && content is ContentPresenter presenter)
            {
                LyricsTile tile = presenter.Child as LyricsTile;

                if (tile != null)
                    return tile.DesiredSize;
            }
        }
        catch (Exception e){}
        
        return new Size(0, 111);
    }
    
    private Size RepeaterSize(int index)
    {
        this._hiddenRepeater.UpdateLayout();
        
        Control content = this._hiddenRepeater.GetOrCreateElement(index);
        content.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

        return content.DesiredSize;
    }

    private int GetIndexOfLyric(LyricPart lyricPart, ObservableCollection<LyricPart> lyricParts) => lyricParts.IndexOf(lyricPart);

    public Thickness GetMargin()
    {
        double m = this._scrollViewer.Viewport.Height / 2.2d;

        m = Math.Floor(m);
        
        return new Thickness(0, m, 0, m);
    }
    
    private void Reset()
    {
        this._debugger.Write("Reset scroller", DebugType.INFO);
        
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            this._scrollViewer.Offset = new Vector(0, 0);
            
            //Resync();
            this.IsSynced = true;
        });
    }
    
    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        if (e.Delta.Y != 0)
        {
            UnSync();
            this._debugger.Write("Detached Scroll-Wheel", DebugType.INFO);
        }

        if (e.Delta.Y > 0)
        {
            this._scrollViewer.Offset = new Vector(0, 
                this._scrollViewer.Offset.Y - 
                this._scrollViewer.SmallChange.Height * 2);
        }
        
        if (e.Delta.Y < 0)
        {
            this._scrollViewer.Offset = new Vector(0, 
                this._scrollViewer.Offset.Y + 
                this._scrollViewer.SmallChange.Height * 2);
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

    public static LyricsScroller Instance
    {
        get => _instance;
    }
    
    public void Resync()
    {
        this._isSyncing = true;
    }
    
    public void Resync(LyricPart part)
    {
        double offset = GetRenderedOffset(part, this._viewModel.Lyrics);
        this._isResyncing = true;

        this._targetLock = part;
        
        Resync();
    }

    public void UnSync()
    {
        this.IsSynced = false;
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

    public bool IsSyncing
    {
        get => _isSyncing;
        set => _isSyncing = value;
    }

    public double Speed
    {
        get => this._speed;
        set => SetField(ref this._speed, value);
    }
}