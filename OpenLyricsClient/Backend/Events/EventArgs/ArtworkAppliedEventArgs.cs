using System;
using OpenLyricsClient.Shared.Structure.Artwork;

namespace OpenLyricsClient.Backend.Events.EventArgs;

public class ArtworkAppliedEventArgs
{
    private Artwork _artwork;

    public ArtworkAppliedEventArgs(Artwork artwork)
    {
        _artwork = artwork;
    }

    public Artwork Artwork
    {
        get => _artwork;
    }
}