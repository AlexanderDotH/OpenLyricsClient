using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using DynamicData;
using OpenLyricsClient.Backend.Structure.Lyrics;

namespace OpenLyricsClient.Frontend.View.Pages.SubPages;

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

    public static readonly StyledProperty<FontWeight> LyricsFontWeightProperty =
        AvaloniaProperty.Register<ScrollPreviewSubPage, FontWeight>(nameof(LyricsFontWeight));
    
    public static readonly StyledProperty<int> LyricsFontSizeProperty =
        AvaloniaProperty.Register<ScrollPreviewSubPage, int>(nameof(LyricsFontSize));
    
    private ObservableCollection<LyricPart> _lyricParts;
    private LyricPart _lyricPart;
    
    public ScrollPreviewSubPage()
    {
        InitializeComponent();
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