using System.Threading.Tasks;
using OpenLyricsClient.Backend.Handler.Song;
using OpenLyricsClient.Shared.Structure;
using OpenLyricsClient.Shared.Structure.Lyrics;
using OpenLyricsClient.Shared.Structure.Song;

namespace OpenLyricsClient.Backend.Collector.Lyrics
{
    interface ILyricsCollector
    {
        Task<LyricData> GetLyrics(SongResponseObject songResponseObject);
        string CollectorName();
        int ProviderQuality();
    }
}
