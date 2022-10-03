using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using DevBase.Async.Task;
using DynamicData;
using OpenLyricsClient.Backend;
using OpenLyricsClient.Backend.Structure.Enum;
using OpenLyricsClient.Backend.Structure.Lyrics;
using OpenLyricsClient.Backend.Utils;
using OpenLyricsClient.Frontend.View.Custom.Model;
using SharpDX.DirectInput;
using Squalr.Engine.Utils.Extensions;

namespace OpenLyricsClient.Frontend.View.Custom.View;

public partial class LyricsScroller : UserControl
{
    public static readonly StyledProperty<LyricData> LyricsProperty =
        AvaloniaProperty.Register<UserControl, LyricData>(nameof(LyricData));
    
    public static readonly StyledProperty<int> SelectedLineProperty =
        AvaloniaProperty.Register<UserControl, int>(nameof(SelectedLine));
    
    public static readonly StyledProperty<LyricPart> CurrentLyricPartProperty =
        AvaloniaProperty.Register<UserControl, LyricPart>(nameof(SelectedLine));
    
    public static readonly StyledProperty<Brush> SelectedLineBrushProperty =
        AvaloniaProperty.Register<UserControl, Brush>(nameof(SelectedLineBrush));

    public static readonly StyledProperty<Brush> UnSelectedLineBrushProperty =
        AvaloniaProperty.Register<UserControl, Brush>(nameof(UnSelectedLineBrush));

    private TaskSuspensionToken _syncScrollerSuspensionToken;
    private double _scrollFrom;
    private double _currentScrollOffset;
    private double _scrollTo;

    private bool _isFirstSync;

    private bool _isInSycedMode;
    
    private LyricsScrollerViewModel _model;

    private ScrollViewer _scrollViewer;
    private ItemsRepeater _itemsRepeater;
    public LyricsScroller()
    {
        InitializeComponent();

        _model = new LyricsScrollerViewModel();
        
        this.DataContext = _model;
        
        this._scrollViewer = this.Get<ScrollViewer>(nameof(CTRL_Viewer));
        this._itemsRepeater = this.Get<ItemsRepeater>(nameof(CTRL_Repeater));

        this._scrollFrom = 0;
        this._currentScrollOffset = 0;
        this._scrollTo = 0;
        this._isInSycedMode = true;
        this._isFirstSync = true;
        
        Core.INSTANCE.TaskRegister.RegisterTask(
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
                if (this._isFirstSync && this.LyricData != null && this.CurrentLyricPart != null)
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
        
        for (int i = 0; i < this.LyricData.LyricParts.Length; i++)
        {
            var child = this._itemsRepeater.TryGetElement(i);
            
            if (i == selectedLine)
            {
                SetTextBlockColor(child, SelectedLineBrush);
                break;
            }
            else
            {
                position += GetVisualSize(i);
                SetTextBlockColor(child, UnSelectedLineBrush);
            }
        }

        this._scrollFrom = this._scrollTo;
        this._scrollTo = CalcOffsetInViewPoint(position, this._scrollViewer, this._itemsRepeater);
    }

    private double CalcOffsetInViewPoint(double selectedLine, ScrollViewer scrollViewer, ItemsRepeater itemsRepeater)
    {
        double singleElement = 36;
        double posTo = SelectedLine * singleElement;

        double sze = 0;
        
        for (int i = SelectedLine - 1; i < SelectedLine; i++)
        {
            if (i > 0 && i < this.LyricData.LyricParts.Length)
            {
                sze += GetVisualSize(i);
            }
        }
        
        return selectedLine - sze;
    }

    private double GetVisualSize(int selectedIndex)
    {
        IControl ctrl = this._itemsRepeater.TryGetElement(selectedIndex);

        if (DataValidator.ValidateData(ctrl))
        {
            if (ctrl is TextBlock)
            {
                TextBlock block = (TextBlock)ctrl;
                return block.TextLayout.Size.Height;
            }            
        }

        return 36;
    }
    
    private void SetTextBlockColor(IControl textBlock, Brush color)
    {
        if (textBlock is TextBlock)
            ((TextBlock)textBlock).Foreground = color;
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
    
    public void Reset()
    {
        this._model.Lyrics.Clear();
        this._itemsRepeater.Children.Clear();
        this._scrollViewer.Offset = new Vector(0, 0);
        this._scrollFrom = 0;
        this._currentScrollOffset = 0;
        this._scrollTo = 0;
        this._isFirstSync = true;
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
    
    public LyricData LyricData
    {
        get => GetValue(LyricsProperty);
        set
        {
            SetValue(LyricsProperty, value);

            if (!AreListsEqual(this._model.Lyrics, value.LyricParts))
            {
                this._model.Lyrics.Clear();
                this._model.Lyrics.AddRange(value.LyricParts);
            }
        }
    }

    public LyricPart CurrentLyricPart
    {
        get { return GetValue(CurrentLyricPartProperty); }
        set
        {
            if (!DataValidator.ValidateData(this.LyricData))
                return;
            
            if (!DataValidator.ValidateData(this.LyricData.LyricParts))
                return;
            
            if (this.LyricData.LyricParts.Length == 0)
                return;
            
            for (int i = 0; i < this.LyricData.LyricParts.Length; i++)
            {
                if (this.LyricData.LyricParts[i] == value)
                {
                    SelectedLine = i;
                    SetValue(CurrentLyricPartProperty, value);
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