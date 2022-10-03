using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
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
using Squalr.Engine.Utils.Extensions;

namespace OpenLyricsClient.Frontend.View.Custom.View;

public partial class LyricsScroller : UserControl
{
    public static readonly StyledProperty<LyricData> LyricsProperty =
        AvaloniaProperty.Register<UserControl, LyricData>(nameof(LyricData));
    
    public static readonly StyledProperty<int> SelectedLineProperty =
        AvaloniaProperty.Register<UserControl, int>(nameof(SelectedLine));
    
    public static readonly StyledProperty<Brush> SelectedLineBrushProperty =
        AvaloniaProperty.Register<UserControl, Brush>(nameof(SelectedLineBrush));

    public static readonly StyledProperty<Brush> UnSelectedLineBrushProperty =
        AvaloniaProperty.Register<UserControl, Brush>(nameof(UnSelectedLineBrush));

    private TaskSuspensionToken _syncScrollerSuspensionToken;
    private double _scrollFrom;
    private double _currentScrollOffset;
    private double _scrollTo;
    
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
        
        Core.INSTANCE.TaskRegister.RegisterTask(
            out _syncScrollerSuspensionToken, 
            new Task(async () => await SyncScrollerTask(), Core.INSTANCE.CancellationTokenSource.Token, TaskCreationOptions.LongRunning), 
            EnumRegisterTypes.SYNC_SCROLLER);
    }

    private async Task SyncScrollerTask()
    {
        double position = 0;
            
        while (true)
        {
            await this._syncScrollerSuspensionToken.WaitForRelease();

            await Task.Delay(1);

            if (!MathUtils.IsDoubleInRange(_currentScrollOffset - 2, _currentScrollOffset + 2, _scrollTo))
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    this._scrollViewer.Offset = new Vector(0, _currentScrollOffset);
                });
            }
            
            if (_currentScrollOffset < _scrollTo)
            {
                _currentScrollOffset += 1;
            }
            else if (_currentScrollOffset < _scrollFrom)
            {
                _currentScrollOffset -= 1;
            }
        }
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
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
    
    private void SetCurrentPosition(int selectedLine)
    {
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
                SetTextBlockColor(child, UnSelectedLineBrush);
            }
        }

        this._scrollFrom = this._scrollTo;
        this._scrollTo = CalcOffsetInViewPoint(selectedLine, this._scrollViewer, this._itemsRepeater);
    }

    private double CalcOffsetInViewPoint(double selectedLine, ScrollViewer scrollViewer, ItemsRepeater itemsRepeater)
    {
        if (itemsRepeater.Children.Count == 0)
            return 0;

        double singleElement = itemsRepeater.Children[0].TransformedBounds.Value.Bounds.Height;
        double posTo = selectedLine * singleElement;
        double itemsInViewPort = Math.Round(scrollViewer.Viewport.Height / singleElement);
        return posTo - ((itemsInViewPort / 2) * singleElement) + singleElement;
    }
    
    private void SetTextBlockColor(IControl textBlock, Brush color)
    {
        if (textBlock is TextBlock)
            ((TextBlock)textBlock).Foreground = color;
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

    public void Reset()
    {
        this._model.Lyrics.Clear();
        this._itemsRepeater.Children.Clear();
        this._scrollViewer.Offset = new Vector(0, 0);
    }
}