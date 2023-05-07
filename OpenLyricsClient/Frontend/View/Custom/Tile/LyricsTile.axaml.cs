using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using OpenLyricsClient.Backend;
using OpenLyricsClient.Backend.Events.EventArgs;
using OpenLyricsClient.Shared.Structure.Lyrics;

namespace OpenLyricsClient.Frontend.View.Custom.Tile;

public partial class LyricsTile : UserControl
{
    private LyricPart _lyricPart;
    private Decorator _decorator;
    
    public LyricsTile()
    {
        AvaloniaXamlLoader.Load(this);

        this._decorator = this.Get<Decorator>(nameof(PART_Decorator));
        
        Core.INSTANCE.LyricHandler.LyricsPercentageUpdated += LyricHandlerOnLyricsPercentageUpdated;
    }

    private void LyricHandlerOnLyricsPercentageUpdated(object sender, LyricsPercentageUpdatedEventArgs args)
    {
        
    }

    public LyricPart Lyric
    {
        get => this._lyricPart;
        set => this._lyricPart = value;
    }
}