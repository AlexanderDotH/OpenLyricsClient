using System.Threading.Tasks;
using DevBase.Api.Apis.Deezer.Structure.Json;
using OpenLyricsClient.Backend.Debugger;
using OpenLyricsClient.Backend.Structure.Lyrics;
using OpenLyricsClient.Backend.Structure.Media;
using OpenLyricsClient.Backend.Structure.Song;
using OpenLyricsClient.Backend.Utils;

namespace OpenLyricsClient.Backend.Collector.Media.Providers.Deezer;

public class DeezerMediaCollector : IMediaCollector
{
    private DevBase.Api.Apis.Deezer.Deezer _deezerApi;
    private Debugger<DeezerMediaCollector> _debugger;

    public DeezerMediaCollector()
    {
        this._debugger = new Debugger<DeezerMediaCollector>(this);
        this._deezerApi = new DevBase.Api.Apis.Deezer.Deezer();
    }
    
    public async Task<MediaData> GetMedia(SongResponseObject songResponseObject)
    {
        if (!DataValidator.ValidateData(songResponseObject))
            return new MediaData();

        if (!DataValidator.ValidateData(songResponseObject.Track))
            return new MediaData();

        if (!DataValidator.ValidateData(songResponseObject.CollectorName))
            return new MediaData();
        
        if (!songResponseObject.CollectorName.Equals(this.CollectorName()))
            return new MediaData();
        
        if (!(songResponseObject.Track is JsonDeezerSearchDataResponse))
            return new MediaData();
        
        JsonDeezerSearchDataResponse track = (JsonDeezerSearchDataResponse)songResponseObject.Track;
        return new MediaData();


    }

    public string CollectorName()
    {
        return "Deezer";
    }

    public int ProviderQuality()
    {
        return 10;
    }
}