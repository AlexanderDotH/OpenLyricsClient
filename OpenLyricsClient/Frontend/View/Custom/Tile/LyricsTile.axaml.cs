using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using OpenLyricsClient.Backend;
using OpenLyricsClient.Backend.Events.EventArgs;
using OpenLyricsClient.Frontend.View.Custom.Tile.Overlays;
using OpenLyricsClient.Shared.Structure.Lyrics;

namespace OpenLyricsClient.Frontend.View.Custom.Tile;

public partial class LyricsTile : UserControl
{
    public static readonly DirectProperty<LyricsTile, LyricPart> LyricPartProperty = 
        AvaloniaProperty.RegisterDirect<LyricsTile, LyricPart>(nameof(LyricPart), o => o.LyricPart, (o, v) => o.LyricPart = v);
    
    private LyricPart _lyricPart;
    private Decorator _decorator;

    private TextOverlay _overlay;

    public LyricsTile()
    {
        AvaloniaXamlLoader.Load(this);

        this._decorator = this.Get<Decorator>(nameof(PART_Decorator));

        this._overlay = new TextOverlay();
        
        Core.INSTANCE.LyricHandler.LyricsPercentageUpdated += LyricHandlerOnLyricsPercentageUpdated;
        Core.INSTANCE.LyricHandler.LyricsFound += LyricHandlerOnLyricsFound;
    }

    public void UpdateViewPort(double width, double height)
    {
        //this._overlay.UpdateViewPort(width, height);
    }
    
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
    }

    private void LyricHandlerOnLyricsFound(object sender)
    {
        //Dispatcher.UIThread.InvokeAsync(() => this._overlay.UpdateViewPort(this.Width, this.Height));
    }

    private void LyricHandlerOnLyricsPercentageUpdated(object sender, LyricsPercentageUpdatedEventArgs args)
    {
        
    }

    public LyricPart LyricPart
    {
        get { return this._lyricPart; }
        set
        {
            this._overlay.LyricPart = value;
            this._decorator.Child = this._overlay;
            
            SetAndRaise(LyricPartProperty, ref _lyricPart, value);
        }
    }
}