using System.Threading.Tasks;
using OpenLyricsClient.Backend.Handler.Song;
using OpenLyricsClient.Backend.Structure;
using OpenLyricsClient.Backend.Structure.Lyrics;
using OpenLyricsClient.Backend.Structure.Song;

namespace OpenLyricsClient.Backend.Collector.Lyrics
{
    interface ICollector
    {
        Task<LyricData> GetLyrics(SongResponseObject songResponseObject);
        string CollectorName();
        int ProviderQuality();
    }
}
