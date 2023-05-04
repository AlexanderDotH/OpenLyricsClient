using System;
using OpenLyricsClient.Shared.Structure.Other;
using SpotifyAPI.Web;

namespace OpenLyricsClient.Shared.Structure
{
    public class SpotifyAccess
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public long SpotifyExpireTime { get; set; }
        public bool IsSpotifyConnected { get; set; }
        public PrivateUser UserData { get; set; }
        public SpotifyStatistics Statistics { get; set; }
    }
}
