using System;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Rendering;
using Avalonia.Threading;
using OpenLyricsClient.Logic;
using OpenLyricsClient.Logic.Settings.Sections.Lyrics;
using OpenLyricsClient.Shared.Structure.Enum;
using OpenLyricsClient.Shared.Structure.Lyrics;
using OpenLyricsClient.Shared.Utils;
using OpenLyricsClient.UI.Models.Elements;

namespace OpenLyricsClient.UI.View.Pages.SubPages;

public partial class ScrollPreviewSubPage : UserControl
{
    public static readonly StyledProperty<int> SelectedLineProperty =
        AvaloniaProperty.Register<ScrollPreviewSubPage, int>(nameof(SelectedLine));

    public static readonly StyledProperty<Brush> SelectedLineBrushProperty =
        AvaloniaProperty.Register<ScrollPreviewSubPage, Brush>(nameof(SelectedLineBrush));

    public static readonly StyledProperty<Brush> UnSelectedLineBrushProperty =
        AvaloniaProperty.Register<ScrollPreviewSubPage, Brush>(nameof(UnSelectedLineBrush));
    
    public static readonly StyledProperty<Thickness> ItemMarginProperty =
        AvaloniaProperty.Register<ScrollPreviewSubPage, Thickness>(nameof(ItemMargin));
    
    public static readonly DirectProperty<ScrollPreviewSubPage, LyricPart> LyricPartProperty = 
        AvaloniaProperty.RegisterDirect<ScrollPreviewSubPage, LyricPart>(nameof(LyricPart), o => o.LyricPart, (o, v) => o.LyricPart = v);
    
    public static readonly DirectProperty<ScrollPreviewSubPage, ObservableCollection<LyricPart>> LyricPartsProperty = 
        AvaloniaProperty.RegisterDirect<ScrollPreviewSubPage, ObservableCollection<LyricPart>>(nameof(LyricParts), o => o.LyricParts, (o, v) => o.LyricParts = v);

    public static readonly DirectProperty<ScrollPreviewSubPage, EnumLyricsDisplayMode> LyricDisplayModeProperty = 
        AvaloniaProperty.RegisterDirect<ScrollPreviewSubPage, EnumLyricsDisplayMode>(nameof(LyricPart), o => o.LyricDisplayMode, (o, v) => o.LyricDisplayMode = v);

    public static readonly StyledProperty<FontWeight> LyricsFontWeightProperty =
        AvaloniaProperty.Register<ScrollPreviewSubPage, FontWeight>(nameof(LyricsFontWeight));
    
    public static readonly StyledProperty<int> LyricsFontSizeProperty =
        AvaloniaProperty.Register<ScrollPreviewSubPage, int>(nameof(LyricsFontSize));

    private ObservableCollection<LyricPart> _lyricParts;
    private LyricPart _lyricPart;
    private EnumLyricsDisplayMode _enumLyricsDisplayMode;

    private ItemsRepeater _itemsRepeater;

    private UiThreadRenderTimer _uiThreadRenderTimer;
    private int _currentSecond;
    private double _currentPercentage;
    
    public ScrollPreviewSubPage()
    {
        InitializeComponent();

        this._itemsRepeater = this.Get<ItemsRepeater>(nameof(CTRL_Repeater));

        this._currentSecond = 0;
        this._currentPercentage = 0;
        this.LyricDisplayMode = Core.INSTANCE.SettingsHandler.Settings<LyricsSection>().GetValue<EnumLyricsDisplayMode>("Selection Mode");
        
        this._uiThreadRenderTimer = new UiThreadRenderTimer(60);
        this._uiThreadRenderTimer.Tick += delegate(TimeSpan span)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                this._currentSecond++;

                if (SelectedLine >= this._lyricParts.Count)
                    SelectedLine = 0;

                for (int i = 0; i < this._lyricParts.Count; i++)
                {
                    LyricsCard card = GetCard(i);

                    if (!DataValidator.ValidateData(card))
                        continue;
                
                    if (this.SelectedLine == i)
                    {
                        this._currentPercentage = Math.Clamp(this._currentPercentage + new Random().Next(0, 5), 0, 100);
                    
                        card.Current = true;
                        card.IgnoreEvents = true;
                        card.Percentage = Math.Clamp(_currentPercentage, 0, 100);
                        card.InvalidateVisual();
                    }
                    else
                    {
                        card.Current = false;
                        card.IgnoreEvents = true;
                        card.Percentage = -1;
                        card.InvalidateVisual();
                    }
                }
            
                if (this._currentSecond % (60 * 2) == 0)
                {
                    SelectedLine++;
                    this._currentPercentage = 0;
                }
            });
        };
    }

    
    
    private LyricsCard GetCard(int index)
    {
        return (LyricsCard)this._itemsRepeater.TryGetElement(index);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    public ObservableCollection<LyricPart> LyricParts
    {
        get { return _lyricParts; }
        set
        {
            SetAndRaise(LyricPartsProperty, ref _lyricParts, value); 
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
            this._lyricPart = value;
            SetAndRaise(LyricPartProperty, ref _lyricPart, value);
        }
    }
    
    public EnumLyricsDisplayMode LyricDisplayMode
    {
        get
        {
            return _enumLyricsDisplayMode;
        }
        set
        {
            this._enumLyricsDisplayMode = value;
            SetAndRaise(LyricDisplayModeProperty, ref _enumLyricsDisplayMode, value);
        }
    }

    public int SelectedLine
    {
        get { return GetValue(SelectedLineProperty); }
        set
        {
            SetValue(SelectedLineProperty, value);
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
}