using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Metrics;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Animators;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using CefNet.Avalonia;
using MusixmatchClientLib.API.Model.Requests;
using OpenLyricsClient.Backend.Structure.Lyrics;

namespace OpenLyricsClient.Frontend.View.Custom;

public partial class LyricsDisplay : UserControl
{
    public static readonly StyledProperty<LyricData> LyricsProperty =
        AvaloniaProperty.Register<UserControl, LyricData>(nameof(LyricData));
    
    public static readonly StyledProperty<int> SelectedLineProperty =
        AvaloniaProperty.Register<UserControl, int>(nameof(SelectedLine));
    
    public static readonly StyledProperty<string> SelectedStringLineProperty =
        AvaloniaProperty.Register<UserControl, string>(nameof(SelectedStringLine));
    
    public static readonly StyledProperty<Brush> SelectedLineBrushProperty =
        AvaloniaProperty.Register<UserControl, Brush>(nameof(SelectedLineBrush));

    public static readonly StyledProperty<Brush> UnSelectedLineBrushProperty =
        AvaloniaProperty.Register<UserControl, Brush>(nameof(UnSelectedLineBrush));
    
    public static readonly StyledProperty<double> ScrollSpeedProperty =
        AvaloniaProperty.Register<UserControl, double>(nameof(ScrollSpeed));

    public ObservableCollection<LyricPart> LyricParts { get; set; }

    private ScrollViewer _scrollViewer;
    private StackPanel _stackPanel;

    public LyricsDisplay()
    {
        LyricParts = new ObservableCollection<LyricPart>();

        InitializeComponent();
        
        this._scrollViewer = this.Get<ScrollViewer>(nameof(CTRL_Viewer));
        this._stackPanel = this.Get<StackPanel>(nameof(CTRL_StackPanel));
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public double ScrollSpeed
    {
        get { return GetValue(ScrollSpeedProperty); }
        set { SetValue(ScrollSpeedProperty, value); }
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
    
    public LyricData LyricData
    {
        get => GetValue(LyricsProperty);
        set
        {
            SetValue(LyricsProperty, value);

            for (int i = 0; i < value.LyricParts.Length; i++)
            {
                this.LyricParts.Add(value.LyricParts[i]);
                this._stackPanel.Children.Add(CreateTextBlock(value.LyricParts[i].Part));
            }
        }
    }

    public string SelectedStringLine
    {
        get { return GetValue(SelectedStringLineProperty); }
        set
        {
            SetValue(SelectedStringLineProperty, value);

            for (int i = 0; i < this.LyricParts.Count; i++)
            {
                LyricPart part = this.LyricParts[i];
                if (part.Part.Equals(value))
                {
                    this.SelectedLine = i;
                }
            }
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

    private void SetCurrentPosition(int selectedLine)
    {
        double position = 0;
        double singleElement = 0;
            
        for (int i = 0; i < this._stackPanel.Children.Count; i++)
        {
            var child = this._stackPanel.Children[i];

            if (i == selectedLine)
            {
                SetTextBlockColor(child, SelectedLineBrush);
                break;
            }
            else
            {
                SetTextBlockColor(child, UnSelectedLineBrush);
                position += child.TransformedBounds.Value.Bounds.Height;
            }
        }
        
        this.ScrollToPosition(this._scrollViewer.Offset.Y ,CalcOffsetInViewPoint(position, this._scrollViewer, this._stackPanel), this._scrollViewer);
    }

    private void SetTextBlockColor(IControl textBlock, Brush color)
    {
        if (textBlock is TextBlock)
            ((TextBlock)textBlock).Foreground = color;
    }

    private double CalcOffsetInViewPoint(double position, ScrollViewer scrollViewer, StackPanel stackPanel)
    {
        if (stackPanel.Children.Count == 0)
            return 0;

        double singleElement = stackPanel.Children[0].TransformedBounds.Value.Bounds.Height;
        double itemsInViewPort = Math.Round(scrollViewer.Viewport.Height / singleElement);
        return position - (singleElement * (itemsInViewPort / 2)) + singleElement;
    }
    
    private TextBlock CreateTextBlock(string lyricLine)
    {
        TextBlock tb = new TextBlock();
        tb.TextWrapping = TextWrapping.Wrap;
        tb.Text = lyricLine;
        tb.FontWeight = FontWeight.Bold;
        tb.FontSize = 30;
        tb.Foreground = this.UnSelectedLineBrush;

        tb.Transitions = new Transitions
        {
            new BrushTransition()
            {
                Property = ForegroundProperty,
                Easing = Easing.Parse("CircularEaseOut"),
                Duration = TimeSpan.FromMilliseconds(500)
            }
        };

        return tb;
    }

    private void ScrollToPosition(double from, double to, ScrollViewer target)
    {
        Task t = Task.Factory.StartNew(async () =>
        {
            double position = from;
            
            while (position < to)
            {
                await Task.Delay(1);

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    target.Offset = new Vector(0, position);
                });

                if (to > from)
                {
                    position += 2;
                }
                else
                {
                    position -= 2;
                }
            }
        });
    }
}