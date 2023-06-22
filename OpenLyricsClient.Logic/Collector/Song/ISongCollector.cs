using OpenLyricsClient.Shared.Structure.Song;

namespace OpenLyricsClient.Logic.Collector.Song;

public interface ISongCollector
{
    Task<SongResponseObject> GetSong(SongRequestObject songRequestObject);
    string CollectorName();
    int ProviderQuality();
}