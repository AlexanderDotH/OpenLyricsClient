using OpenLyricsClient.Shared.Structure.Artwork;

namespace OpenLyricsClient.Logic.Events.EventArgs;

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