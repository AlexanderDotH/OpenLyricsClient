using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevBase.Generic;
using LyricsWPF.Backend.Collector;
using LyricsWPF.Backend.Romanisation;
using LyricsWPF.Backend.Structure;
using SpotifyApi.NetCore.Authorization;

namespace LyricsWPF.Backend.Settings
{
    public class Settings
    {
        public SpotifyAccess SpotifyAccess { get; set; }

        public TidalAccess TidalAccess { get; set; }
        public List<RomanizeSelection> RomanizeSelection { get; set; }
        public SelectionMode LyricSelectionMode { get; set; }
    }
}
