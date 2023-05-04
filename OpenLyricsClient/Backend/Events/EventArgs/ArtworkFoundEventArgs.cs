using System;
using OpenLyricsClient.Shared.Structure.Artwork;
using OpenLyricsClient.Shared.Structure.Song;

namespace OpenLyricsClient.Backend.Events.EventArgs;

public class ArtworkFoundEventArgs : System.EventArgs
{
    private Artwork _artwork;
    private SongRequestObject _songRequestObject;
    
    public ArtworkFoundEventArgs(Artwork artwork, SongRequestObject songRequestObject)
    {
        this._artwork = artwork;
        this._songRequestObject = songRequestObject;
    }

    public Artwork Artwork
    {
        get => _artwork;
    }

    public SongRequestObject SongRequestObject
    {
        get => this._songRequestObject;
    }
}