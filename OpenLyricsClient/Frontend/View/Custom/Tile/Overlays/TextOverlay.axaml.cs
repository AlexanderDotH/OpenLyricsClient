using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using OpenLyricsClient.Frontend.Models.Custom.Tile.Overlays;
using OpenLyricsClient.Shared.Structure.Lyrics;

namespace OpenLyricsClient.Frontend.View.Custom.Tile.Overlays;

public partial class TextOverlay : UserControl
{
    public static readonly DirectProperty<TextOverlay, LyricPart> LyricPartProperty = 
        AvaloniaProperty.RegisterDirect<TextOverlay, LyricPart>(nameof(LyricPart), o => o.LyricPart, (o, v) => o.LyricPart = v);

    private LyricPart _lyricPart;
    private ItemsControl _itemsControl;
    
    private TextOverlayViewModel _viewModel;
    
    public TextOverlay()
    {
        InitializeComponent();
        
        this._viewModel = DataContext as TextOverlayViewModel;

        this._itemsControl = this.Get<ItemsControl>(nameof(PART_Items));
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    public void UpdateViewPort(double width, double height)
    {
        this._viewModel.UpdateLyricsWrapping(width, height);
    }
    
    public LyricPart LyricPart
    {
        get { return this._lyricPart; }
        set
        {
            SetAndRaise(LyricPartProperty, ref _lyricPart, value);
            this._viewModel.LyricPart = value;
        }
    }
}