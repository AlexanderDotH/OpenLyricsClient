using System;
using SpotifyApi.NetCore.Authorization;

namespace OpenLyricsClient.Backend.Structure
{
    public class SpotifyAccess
    {
        public BearerAccessToken BearerAccess { get; set; }
        public string RefreshToken { get; set; }
        public DateTime SpotifyExpireTime { get; set; }
        public bool IsSpotifyConnected { get; set; }
    }
}
