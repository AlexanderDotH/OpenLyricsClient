using SpotifyAPI.Web;

namespace OpenLyricsClient.Shared.Structure.Other;

public class SpotifyStatistics
{
    public SimpleArtist[] TopArtists { get; set; }
        
    public SimpleTrack[] TopTracks { get; set; }
}