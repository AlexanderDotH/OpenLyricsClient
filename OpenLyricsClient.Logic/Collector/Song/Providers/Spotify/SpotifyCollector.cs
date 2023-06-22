using OpenLyricsClient.Shared.Structure.Enum;
using OpenLyricsClient.Shared.Structure.Song;
using OpenLyricsClient.Shared.Utils;
using SpotifyAPI.Web;

namespace OpenLyricsClient.Logic.Collector.Song.Providers.Spotify;

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