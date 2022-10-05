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
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.Utilities;
using DevBase.Async.Task;
using DynamicData;
using OpenLyricsClient.Backend;
using OpenLyricsClient.Backend.Events;
using OpenLyricsClient.Backend.Events.EventArgs;
using OpenLyricsClient.Backend.Structure.Enum;
using OpenLyricsClient.Backend.Structure.Lyrics;
using OpenLyricsClient.Backend.Structure.Song;
using OpenLyricsClient.Backend.Utils;
using SharpDX.DirectInput;
using Squalr.Engine.Utils.Extensions;

namespace OpenLyricsClient.Frontend.View.Custom.View;

public partial class LyricsScroller : UserControl
{
    public static readonly StyledProperty<int> SelectedLineProperty =
        AvaloniaProperty.Register<UserControl, int>(nameof(SelectedLine));

    public static readonly StyledProperty<Brush> SelectedLineBrushProperty =
        AvaloniaProperty.Register<UserControl, Brush>(nameof(SelectedLineBrush));

    public static readonly StyledProperty<Brush> UnSelectedLineBrushProperty =
        AvaloniaProperty.Register<UserControl, Brush>(nameof(UnSelectedLineBrush));
    
    public static readonly StyledProperty<Thickness> ItemMarginProperty =
        AvaloniaProperty.Register<UserControl, Thickness>(nameof(ItemMargin));
    
    public static readonly DirectProperty<LyricsScroller, LyricPart> LyricPartProperty = 
        AvaloniaProperty.RegisterDirect<LyricsScroller, LyricPart>(nameof(LyricPart), o => o.LyricPart, (o, v) => o.LyricPart = v);
    
    public static readonly DirectProperty<LyricsScroller, ObservableCollection<LyricPart>> LyricPartsProperty = 
        AvaloniaProperty.RegisterDirect<LyricsScroller, ObservableCollection<LyricPart>>(nameof(LyricParts), o => o.LyricParts, (o, v) => o.LyricParts = v);

    private ObservableCollection<LyricPart> _lyricParts;
    private LyricPart _lyricPart;

    private TaskSuspensionToken _syncScrollerSuspensionToken;
    private double _scrollFrom;
    private double _currentScrollOffset;
    private double _scrollTo;

    private bool _isFirstSync;

    private bool _isInSycedMode;
    
    private ScrollViewer _scrollViewer;
    private ItemsRepeater _itemsRepeater;

    public LyricsScroller()
    {
        InitializeComponent();

        this.DataContext = new LyricsScrollerViewModel();
        
        this._scrollViewer = this.Get<ScrollViewer>(nameof(CTRL_Viewer));
        this._itemsRepeater = this.Get<ItemsRepeater>(nameof(CTRL_Repeater));

        this._scrollFrom = 0;
        this._currentScrollOffset = 0;
        this._scrollTo = 0;
        this._isInSycedMode = true;
        this._isFirstSync = true;

        Core.INSTANCE.TaskRegister.Register(
            out _syncScrollerSuspensionToken, 
            new Task(async () => await SyncScrollerTask(), Core.INSTANCE.CancellationTokenSource.Token, TaskCreationOptions.LongRunning), 
            EnumRegisterTypes.SYNC_SCROLLER);
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private async Task SyncScrollerTask()
    {
        while (!Core.IsDisposed())
        {
            await this._syncScrollerSuspensionToken.WaitForRelease();

            await Task.Delay(1);
            
            if (!_isInSycedMode)
                continue;

            if (!MathUtils.IsDoubleInRange(_currentScrollOffset - 2, _currentScrollOffset + 2, _scrollTo) && _isInSycedMode)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    this._scrollViewer.Offset = new Vector(0, _currentScrollOffset);
                });
            }

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (this._isFirstSync && this._lyricParts != null && this._lyricPart != null)
                {
                    this._scrollViewer.Offset = new Vector(0, _scrollTo);
                    this._currentScrollOffset = this._scrollTo;
                    this._isFirstSync = false;
                }
            });

            if (_currentScrollOffset < _scrollTo)
            {
                _currentScrollOffset += 1;
            }
            else
            {
                _currentScrollOffset -= 1;
            }
        }
    }

    private void SetCurrentPosition(int selectedLine)
    {
        double position = 0;
        
        TryUnselectAll();
        
        for (int i = 0; i < this._lyricParts.Count; i++)
        {
            var child = this._itemsRepeater.TryGetElement(i);
            
            if (i == selectedLine)
            {
                SetTextBlockColor(child, SelectedLineBrush);
                break;
            }
            else
            {
                position += GetVisualSize(i, ItemMargin.Bottom);
                SetTextBlockColor(child, UnSelectedLineBrush);
            }
        }

        this._scrollFrom = this._scrollTo;
        this._scrollTo = CalcOffsetInViewPoint(position);
    }

    private double CalcOffsetInViewPoint(double selectedLine)
    {
        double sze = 0;
        
        for (int i = SelectedLine - 1; i < SelectedLine; i++)
        {
            if (i >= 0 && i <= this._lyricParts.Count)
            {
                sze += GetVisualSize(i, ItemMargin.Bottom);
            }
        }
        
        return selectedLine - sze;
    }

    private double GetVisualSize(int selectedIndex, double gapSize)
    {
        IControl ctrl = this._itemsRepeater.TryGetElement(selectedIndex);

        if (DataValidator.ValidateData(ctrl))
        {
            if (ctrl is TextBlock)
            {
                TextBlock block = (TextBlock)ctrl;
                return block.TextLayout.Size.Height + gapSize;
            }            
        }

        return 36 + gapSize;
    }
    
    private void SetTextBlockColor(IControl textBlock, Brush color)
    {
        if (textBlock is TextBlock)
            ((TextBlock)textBlock).Foreground = color;
    }

    private void TryUnselectAll()
    {
        int totalCount = -1;
        
        this._itemsRepeater.TryGetTotalCount(out totalCount);
        
        if (totalCount == -1)
            return;
        
        for (int i = 0; i < totalCount; i++)
        {
            IControl ctrl = this._itemsRepeater.TryGetElement(i);

            if (DataValidator.ValidateData(ctrl))
            {
                if (ctrl is TextBlock)
                {
                    SetTextBlockColor(ctrl, UnSelectedLineBrush);
                }            
            }
        }
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
                    _lyricPart = value;
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

    private void CTRL_Viewer_OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
    }

    private void CTRL_Viewer_OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        var nick = e.ExtentDelta.SquaredLength + e.OffsetDelta.SquaredLength;

        if (nick > 10000000)
        {
            this._isFirstSync = true;
            return;
        }

    }
}

public class LyricsScrollerViewModel : INotifyPropertyChanged
{
    private TaskSuspensionToken _displayLyricsSuspensionToken;
    private TaskSuspensionToken _syncLyricsSuspensionToken;

    public ObservableCollection<LyricPart> _lyricParts;
    private LyricPart _lyricPart;

    public LyricsScrollerViewModel()
    {
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
        }
    }

    private async Task DisplayLyricsTask()
    {
        while (!Core.IsDisposed())
        {
            await Task.Delay(1);
            await this._displayLyricsSuspensionToken.WaitForRelease();

            Song currentSong = Core.INSTANCE.SongHandler.CurrentSong;

            if (!DataValidator.ValidateData(currentSong))
                continue;

            if (!DataValidator.ValidateData(currentSong.Lyrics))
                continue;

            if (!AreListsEqual(this.CurrentLyricParts, currentSong.Lyrics.LyricParts))
            {
                this.CurrentLyricParts =  new ObservableCollection<LyricPart>(currentSong.Lyrics.LyricParts);
            }
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
        }
    }
    
    private bool AreListsEqual(ObservableCollection<LyricPart> lyricPartsList1, LyricPart[] lyricPartsList2)
    {
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