using System.Threading.Tasks;
using OpenLyricsClient.Backend.Handler.Song;
using OpenLyricsClient.Backend.Structure;

namespace OpenLyricsClient.Backend.Collector.Lyrics
{
    interface ICollector
    {
        Task<LyricData> GetLyrics(SongRequestObject songRequestObject);
        string CollectorName();
        int ProviderQuality();
    }
}
