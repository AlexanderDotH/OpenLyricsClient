using System.Threading.Tasks;
using OpenLyricsClient.Shared.Structure.Lyrics;
using OpenLyricsClient.Shared.Structure.Media;
using OpenLyricsClient.Shared.Structure.Song;

namespace OpenLyricsClient.Backend.Collector.Media;

public interface IMediaCollector
{
    Task<MediaData> GetMedia(SongResponseObject songResponseObject);
    string CollectorName();
    int ProviderQuality();
}