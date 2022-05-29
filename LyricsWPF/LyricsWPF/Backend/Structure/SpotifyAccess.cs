using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpotifyApi.NetCore.Authorization;

namespace LyricsWPF.Backend.Structure
{
    public class SpotifyAccess
    {
        public BearerAccessToken BearerAccess { get; set; }
        public string RefreshToken { get; set; }
        public DateTime SpotifyExpireTime { get; set; }
        public bool IsSpotifyConnected { get; set; }
    }
}
