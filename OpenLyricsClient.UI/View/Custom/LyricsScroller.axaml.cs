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
using Avalonia.Animation;
using Avalonia.Animation.Easings;
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
using OpenLyricsClient.Logic.Settings.Sections.Lyrics;
using OpenLyricsClient.Shared.Structure.Enum;
using OpenLyricsClient.Shared.Structure.Lyrics;
using OpenLyricsClient.Shared.Structure.Visual;
using OpenLyricsClient.Shared.Utils;
using OpenLyricsClient.UI.Animation;
using OpenLyricsClient.UI.Models.Custom;
using OpenLyricsClient.UI.Scaling;
using OpenLyricsClient.UI.Structure.Enum;
using OpenLyricsClient.UI.View.Custom.Tile;
using OpenLyricsClient.UI.View.Windows;
using Squalr.Engine.Utils.Extensions;
using Debug = System.Diagnostics.Debug;
using ScrollChangedEventArgs = Avalonia.Controls.ScrollChangedEventArgs;

namespace OpenLyricsClient.UI.View.Custom;

public partial class LyricsScroller : UserControl, INotifyPropertyChanged
{
    // Styled Properties
    public static readonly StyledProperty<bool> IsSyncedProperty =
        AvaloniaProperty.Register<LyricsScroller, bool>(nameof(IsSynced));

    // Controls
    private ScrollViewer _scrollViewer;
    private ItemsControl _itemsControl;
    private Grid _visualElementsGrid;
    private Grid _layoutGrid;
    
    // ViewModel
    private LyricsScrollerViewModel _viewModel;
    
    // Instance
    private static LyricsScroller _instance;
    private VectorTransition _transition;

    // Multithreadding
    private TaskSuspensionToken _suspensionToken;
    private UiThreadRenderTimer _uiThreadRenderTimer;

    private CancellationTokenSource _renderTimerCancellationTokenSource;
    
    // Variables
    private double _speed;
    private bool _isPointerPressed;

    private bool _isScrollerReady;
    
    private LyricPart _lastPart;

    private List<ScrollerElement> _visualElementsList;
    private List<ScrollerElement> _missingVisualElementsList;
    private Size _noteElementSize;

    private Margin _itemMargin;
    
    private Debugger<LyricsScroller> _debugger;

    public event PropertyChangedEventHandler? PropertyChanged;
    
    public LyricsScroller()
    {
        AvaloniaXamlLoader.Load(this);

        this._debugger = new Debugger<LyricsScroller>(this);

        _instance = this;

        this._isPointerPressed = false;
        this._isScrollerReady = false;

        this._noteElementSize = new Size(-1, -1);
        this._missingVisualElementsList = new List<ScrollerElement>();
        
        this._itemMargin = Core.INSTANCE.SettingsHandler.Settings<LyricsSection>().GetValue<Margin>("Lyrics Margin");
        
        Reset();

        this._lastPart = new LyricPart(-1, "Never Gonna Give Me Up");

        this._visualElementsList = new List<ScrollerElement>();
        
        this.DataContext = new LyricsScrollerViewModel();
        this._viewModel = this.DataContext as LyricsScrollerViewModel;
        
        this._itemsControl = this.Get<ItemsControl>(nameof(CTRL_Items));
        this._scrollViewer = this.Get<ScrollViewer>(nameof(CTRL_Viewer));
        this._visualElementsGrid = this.Get<Grid>(nameof(CTRL_VisualElements));
        this._layoutGrid = this.Get<Grid>(nameof(CTRL_Layout));
        
        this.EffectiveViewportChanged += OnEffectiveViewportChanged;
        
        MainWindow.Instance.PointerPressed += InstanceOnPointerPressed;
        MainWindow.Instance.PointerReleased += InstanceOnPointerReleased;
        
        MainWindow.Instance.PropertyChanged += InstanceOnPropertyChanged;
        
        MainWindow.Instance.PointerWheelChanged += OnPointerWheelChanged;
        this._itemsControl.PointerWheelChanged += OnPointerWheelChanged;
        this._layoutGrid.PointerWheelChanged += OnPointerWheelChanged;
        this._visualElementsGrid.PointerWheelChanged += OnPointerWheelChanged;
        
        AttachedToVisualTree += OnAttachedToVisualTree;
        
        ApplyTransitionSpeed(CalculateSpeedToTimeSpan(50, TimeSpan.FromSeconds(1.5)));
    }

    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        Core.INSTANCE.LyricHandler.LyricsFound += LyricHandlerOnLyricsFound;
        Core.INSTANCE.LyricHandler.LyricChanged += LyricHandlerOnLyricChanged;
        Core.INSTANCE.SongHandler.SongChanged += SongHandlerOnSongChanged;
        
        Core.INSTANCE.SettingsHandler.SettingsChanged += SettingsHandlerOnSettingsChanged;
    }

    private void SettingsHandlerOnSettingsChanged(object sender, SettingsChangedEventArgs settingschangedeventargs)
    {
        if (settingschangedeventargs.Field.SequenceEqual("Lyrics Margin"))
        {
            Margin t = Core.INSTANCE.SettingsHandler.Settings<LyricsSection>().GetValue<Margin>("Lyrics Margin");
            this._itemMargin = t;
            
            // I don't wanna call that here, but I'm not sure if I can remove that
            this.FillVisualElements();
        }
    }

    private void LyricHandlerOnLyricChanged(object sender, LyricChangedEventArgs lyricchangedeventargs)
    {
        if (lyricchangedeventargs.LyricPart.Equals(this._lastPart)) 
            return;

        if (!this.IsSynced)
            return;
        
        double offset = GetRenderedOffset(lyricchangedeventargs.LyricPart, this._viewModel.Lyrics);
        this._itemsControl.Margin = GetMargin();
        this._scrollViewer.Offset = new Vector(0, offset);

        this._isScrollerReady = true;
        
        this._lastPart = lyricchangedeventargs.LyricPart;
    }
    
    private void InstanceOnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        Type propertyType = e.Property.PropertyType;
            
        if (propertyType == typeof(WindowState))
            this._isPointerPressed = false;
    }

    private void SongHandlerOnSongChanged(object sender, SongChangedEventArgs songchangedevent)
    {
        if (songchangedevent.EventType != EventType.PRE)
            return;
    
        Reset();
    }

    private void LyricHandlerOnLyricsFound(object sender, LyricsFoundEventArgs args)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            this._scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            
            ApplyTransitionSpeed(CalculateSpeedToTimeSpan(args.LyricData.LyricSpeed, TimeSpan.FromSeconds(1.5)));
            
            this.FillVisualElements();
            
            ReSync();
        });
    }

    private void InstanceOnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        this._isPointerPressed = true;
    }

    private void InstanceOnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        this._isPointerPressed = false;
    }

    private void OnEffectiveViewportChanged(object? sender, EffectiveViewportChangedEventArgs e)
    {
        if (this._isPointerPressed)
            this.FillVisualElements();
    }
    
    private void ApplyTransitionSpeed(TimeSpan span)
    {
        Transitions transitions = new Transitions();

        VectorTransition vectorTransition = new VectorTransition
        {
            Property = ScrollViewer.OffsetProperty,
            Duration = span,
            Easing = new QuinticEaseInOut()
        };
        
        transitions.Add(vectorTransition);

        this._transition = vectorTransition;
        
        this._scrollViewer.Transitions = transitions;
    }
    
    private TimeSpan CalculateSpeedToTimeSpan(double percentage, TimeSpan maxTimeSpan)
    {
        double multiplier = 1.0d;

        double max = maxTimeSpan.TotalMilliseconds;
        double x = percentage * (max * 0.01);

        if (double.IsNaN(x))
            x = 20;
        
        double y = max - x;

        y = Math.Clamp(y, 0, max);
        y = Math.Abs(y);

        double result = y * multiplier;
        
        return TimeSpan.FromMilliseconds(result);
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

        double halfHeight = this._scrollViewer.Viewport.Height / 2.5;
        
        position -= halfHeight;
        position += margin.Top;

        return position;
    }

    private Size GetRenderedSize(int index)
    {
        if (index > this._visualElementsList.Count)
            return new Size();
        
        for (int i = 0; i < this._visualElementsList.Count; i++)
        {
            var e = this._visualElementsList[i];

            if (e.Index == index)
                return e.Size;
        }
        
        return new Size();
    }
    
    private void FillVisualElements()
    {
        this._itemsControl.Items.Clear();
        this._visualElementsGrid.Children.Clear();
        this._visualElementsList.Clear();
        this._missingVisualElementsList.Clear();
        
        this._noteElementSize = new Size(0, 0);
        
        this._visualElementsGrid.IsVisible = true;
        
        for (var i = 0; i < this._viewModel.Lyrics.Count; i++)
        {
            LyricPart part = this._viewModel.Lyrics[i];

            LyricsTile dummyTile = new LyricsTile()
            {
                LyricPart = part,
                Headless = true
            };
            
            if (dummyTile.ElementType == EnumElementType.NOTE &&
                !this._missingVisualElementsList.IsNullOrEmpty())
            {
                this._missingVisualElementsList.Add(new ScrollerElement() {Index = i});
                continue;
            }
            
            if (dummyTile.ElementType == EnumElementType.NOTE)
                this._missingVisualElementsList.Add(new ScrollerElement() {Index = i});
            
            dummyTile.LayoutUpdated += TileOnLayoutUpdated;
                
            this._visualElementsGrid.Children.Add(dummyTile);
        }
        
        this._itemsControl.UpdateLayout();
    }

    private void TileOnLayoutUpdated(object? sender, EventArgs e)
    {
        if (sender is LyricsTile tile)
        {
            if (tile.DesiredSize.Height == this._itemMargin.Bottom || tile.DesiredSize.Height == 0)
                return;
            
            int index = GetIndexOfLyric(tile.LyricPart, this._viewModel.Lyrics);
            
            if (VisualContainsIndex(index))
                return;

            if (tile.ElementType == EnumElementType.NOTE)
                this._noteElementSize = tile.DesiredSize;
            
            this._visualElementsList.Add(new ScrollerElement()
            {
                Index = index,
                Size = tile.DesiredSize
            });

            tile.IsVisible = false;

            if ((this._visualElementsList.Count + this._missingVisualElementsList.Count) == 
                this._viewModel.Lyrics.Count)
            {
                this._visualElementsGrid.IsVisible = false;
                this._visualElementsGrid.Children.Clear();
                
                FillRealItems();
                
                CalculateMissingElements();
            }
        }
    }

    private void FillRealItems()
    {
        for (int i = 0; i < this._viewModel.Lyrics.Count; i++)
        {
            LyricsTile realTile = new LyricsTile()
            {
                LyricPart = this._viewModel.Lyrics[i]
            };
                
            this._itemsControl.Items.Add(realTile);
        }
    }

    private void CalculateMissingElements()
    {
        for (int i = 0; i < this._missingVisualElementsList.Count; i++)
            this._missingVisualElementsList[i].Size = this._noteElementSize;
        
        this._visualElementsList.AddRange(this._missingVisualElementsList);
    }

    private bool VisualContainsIndex(int index)
    {
        for (var i = 0; i < this._visualElementsList.Count; i++)
        {
            ScrollerElement element = this._visualElementsList[i];
            
            if (element.Index == index)
                return true;
        }

        return false;
    }
    
    private int GetIndexOfLyric(LyricPart lyricPart, ObservableCollection<LyricPart> lyricParts) => lyricParts.IndexOf(lyricPart);

    public Thickness GetMargin()
    {
        double m = this._scrollViewer.Viewport.Height / 2.5d;
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
    
    private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
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
    
    public void UnSync()
    {
        this.IsSynced = false;

        this._scrollViewer.Transitions = null;
    }

    public void ReSync()
    {
        Transitions transitions = new Transitions();
        transitions.Add(this._transition);
        
        double offset = GetRenderedOffset(this._viewModel.Lyric, this._viewModel.Lyrics);
        
        this._scrollViewer.Transitions = transitions;
        
        this.IsSynced = true;
        this._scrollViewer.Offset = new Vector(0, offset);
    }

    public void AllowSync()
    {
        Transitions transitions = new Transitions();
        transitions.Add(this._transition);
        this._scrollViewer.Transitions = transitions;

        IsSynced = true;
    }

    public bool IsScrollerReady
    {
        get => _isScrollerReady;
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
    
    public static LyricsScroller Instance
    {
        get => _instance;
    }
}