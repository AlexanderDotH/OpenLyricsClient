using System;
using SpotifyAPI.Web;

namespace OpenLyricsClient.Backend.Structure
{
    public class SpotifyAccess
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public long SpotifyExpireTime { get; set; }
        public bool IsSpotifyConnected { get; set; }
        public PrivateUser UserData { get; set; }
    }
}
