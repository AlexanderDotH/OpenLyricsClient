using OpenLyricsClient.Shared.Structure.Lyrics;
using OpenLyricsClient.Shared.Structure.Song;

namespace OpenLyricsClient.Logic.Collector.Lyrics
{
    interface ILyricsCollector
    {
        Task<LyricData> GetLyrics(SongResponseObject songResponseObject);
        string CollectorName();
        int ProviderQuality();
    }
}
