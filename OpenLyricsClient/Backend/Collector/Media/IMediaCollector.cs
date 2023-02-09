using System.Threading.Tasks;
using OpenLyricsClient.Backend.Structure.Lyrics;
using OpenLyricsClient.Backend.Structure.Media;
using OpenLyricsClient.Backend.Structure.Song;

namespace OpenLyricsClient.Backend.Collector.Media;

public interface IMediaCollector
{
    Task<MediaData> GetMedia(SongResponseObject songResponseObject);
    string CollectorName();
    int ProviderQuality();
}