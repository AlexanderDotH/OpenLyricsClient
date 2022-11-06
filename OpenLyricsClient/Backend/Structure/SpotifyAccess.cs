using System;

namespace OpenLyricsClient.Backend.Structure
{
    public class SpotifyAccess
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public int SpotifyExpireTime { get; set; }
        public bool IsSpotifyConnected { get; set; }
    }
}
