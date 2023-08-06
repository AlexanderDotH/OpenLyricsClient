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
using OpenLyricsClient.Logic.Settings.Sections.Lyrics;
using OpenLyricsClient.Shared.Structure.Enum;
using OpenLyricsClient.Shared.Structure.Lyrics;
using OpenLyricsClient.Shared.Structure.Visual;
using OpenLyricsClient.Shared.Utils;
using OpenLyricsClient.UI.Animation;
using OpenLyricsClient.UI.Models.Custom;
using OpenLyricsClient.UI.Structure.Enum;
using OpenLyricsClient.UI.View.Custom.Tile;
using OpenLyricsClient.UI.View.Windows;
using Debug = System.Diagnostics.Debug;

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
    private bool _isPointerPressed;

    private LyricPart _lastPart;

    private List<(int, Size)> _visualElementsList;

    private Margin _itemMargin;
    
    private Debugger<LyricsScroller> _debugger;

    public event PropertyChangedEventHandler? PropertyChanged;
    
    public LyricsScroller()
    {
        AvaloniaXamlLoader.Load(this);

        this._debugger = new Debugger<LyricsScroller>(this);

        _instance = this;

        this._isPointerPressed = false;
        
        this._itemMargin = Core.INSTANCE.SettingsHandler.Settings<LyricsSection>().GetValue<Margin>("Lyrics Margin");
        
        this.Speed = 15 * 0.1;

        this._isSyncing = false;

        Reset();

        this._visualElementsList = new List<(int, Size)>();
        
        this.DataContext = new LyricsScrollerViewModel();
        this._viewModel = this.DataContext as LyricsScrollerViewModel;
        
        this._itemsControl = this.Get<ItemsControl>(nameof(CTRL_Items));
        this._scrollViewer = this.Get<ScrollViewer>(nameof(CTRL_Viewer));
        this._visualElementsGrid = this.Get<Grid>(nameof(CTRL_VisualElements));
        
        this.EffectiveViewportChanged += OnEffectiveViewportChanged;
        
        MainWindow.Instance.PointerPressed += InstanceOnPointerPressed;
        MainWindow.Instance.PointerReleased += InstanceOnPointerReleased;
        
        AttachedToVisualTree += OnAttachedToVisualTree;
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
        Debug.WriteLineIf(this._isPointerPressed, "Pressed");
        
        if (!this._isPointerPressed)
            this.FillVisualElements();
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
            this.FillVisualElements();
        });
        
        this.Speed = args.LyricData.LyricSpeed * 0.1f;
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

        Dispatcher.UIThread.InvokeAsync(() =>
        {
            double offset = GetRenderedOffset(lyricchangedeventargs.LyricPart, this._viewModel.Lyrics);
            this._itemsControl.Margin = GetMargin();
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

            if (e.Item1 == index)
                return e.Item2;
        }
        
        return new Size();
    }

    private void FillVisualElements()
    {
        this._itemsControl.Items.Clear();
        this._visualElementsGrid.Children.Clear();
        this._visualElementsList.Clear();
        
        this._visualElementsGrid.IsVisible = true;
        
        for (var i = 0; i < this._viewModel.Lyrics.Count; i++)
        {
            LyricPart part = this._viewModel.Lyrics[i];

            LyricsTile dummyTile = new LyricsTile()
            {
                LyricPart = part,
                Headless = true
            };

            dummyTile.LayoutUpdated += TileOnLayoutUpdated;
                
            this._visualElementsGrid.Children.Add(dummyTile);

            LyricsTile realTile = new LyricsTile()
            {
                LyricPart = part
            };
                
            this._itemsControl.Items.Add(realTile);
        }
        
        this._scrollViewer.UpdateLayout();
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

            this._visualElementsList.Add((index, tile.DesiredSize));

            if (this._visualElementsList.Count == this._viewModel.Lyrics.Count)
                this._visualElementsGrid.IsVisible = false;
        }
    }

    private bool VisualContainsIndex(int index)
    {
        for (var i = 0; i < this._visualElementsList.Count; i++)
        {
            (int, Size) element = this._visualElementsList[i];
            
            if (element.Item1 == index)
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