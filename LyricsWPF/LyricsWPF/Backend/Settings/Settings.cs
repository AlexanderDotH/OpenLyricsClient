using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevBase.Generic;
using LyricsWPF.Backend.Collector;
using SpotifyApi.NetCore.Authorization;

namespace LyricsWPF.Backend.Settings
{
    public class Settings
    {
        public BearerAccessRefreshToken BearerAccess { get; set; }
        public DateTime? SpotifyExpireTime { get; set; }
        public List<RomanizeSelection> RomanizeSelection { get; set; }
        public SelectionMode LyricSelectionMode { get; set; }
        public bool IsSpotifyConnected { get; set; }
    }
}
