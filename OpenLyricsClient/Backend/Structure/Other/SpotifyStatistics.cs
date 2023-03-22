using SpotifyAPI.Web;

namespace OpenLyricsClient.Backend.Structure.Other;

public class SpotifyStatistics
{
    public FullArtist[] TopArtists { get; set; }
        
    public FullTrack[] TopTracks { get; set; }
}