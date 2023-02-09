using System.Collections.Generic;
using OpenLyricsClient.Backend.Romanization;
using OpenLyricsClient.Backend.Structure;
using OpenLyricsClient.Backend.Structure.Enum;

namespace OpenLyricsClient.Backend.Settings
{
    public class Settings
    {
        public SpotifyAccess SpotifyAccess { get; set; }
        public TidalAccess TidalAccess { get; set; }
        public List<RomanizeSelection> RomanizeSelection { get; set; }
        public List<MusixMatchToken> MusixMatchToken { get; set; }
        
    }
}
