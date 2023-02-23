using System.Threading.Tasks;
using OpenLyricsClient.Backend.Structure.Enum;
using OpenLyricsClient.Backend.Structure.Song;
using OpenLyricsClient.Backend.Utils;
using SpotifyAPI.Web;

namespace OpenLyricsClient.Backend.Collector.Song.Providers.Spotify;

public class SpotifyCollector : ISongCollector
{
    public async Task<SongResponseObject> GetSong(SongRequestObject songRequestObject)
    {
        if (!(DataValidator.ValidateData(songRequestObject) &&
              DataValidator.ValidateData(songRequestObject.Artists, songRequestObject.SongDuration,
                  songRequestObject.SongName, songRequestObject.Album)))
            return null;

        if (songRequestObject.Song.DataOrigin != DataOrigin.SPOTIFY)
            return null;

        if (!(songRequestObject.Song.TrackObject is FullTrack))
            return null;
        
        FullTrack track = (FullTrack)songRequestObject.Song.TrackObject;
        return new SongResponseObject(songRequestObject, track, this.CollectorName());
    }

    public string CollectorName()
    {
        return "Spotify";
    }

    public int ProviderQuality()
    {
        return 10;
    }
}