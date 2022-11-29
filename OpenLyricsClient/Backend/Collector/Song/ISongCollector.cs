using System.Threading.Tasks;
using OpenLyricsClient.Backend.Structure.Song;

namespace OpenLyricsClient.Backend.Collector.Song;

public interface ISongCollector
{
    Task<SongResponseObject> GetSong(SongRequestObject songRequestObject);
    string CollectorName();
    int ProviderQuality();
}