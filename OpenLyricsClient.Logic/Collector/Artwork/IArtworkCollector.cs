using OpenLyricsClient.Shared.Structure.Song;

namespace OpenLyricsClient.Logic.Collector.Artwork
{
    interface IArtworkCollector
    {
        Task<Shared.Structure.Artwork.Artwork> GetArtwork(SongResponseObject songResponseObject);
        string CollectorName();
        int Quality();

    }
}
