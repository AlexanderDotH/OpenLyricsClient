using System.Collections.Generic;
using DevBaseApi.Apis.Tidal.Structure.Json;
using OpenLyricsClient.Backend.Collector.Lyrics;
using OpenLyricsClient.Backend.Romanization;
using OpenLyricsClient.Backend.Structure;

namespace OpenLyricsClient.Backend.Settings
{
    public class Settings
    {
        public SpotifyAccess SpotifyAccess { get; set; }
        public TidalAccess TidalAccess { get; set; }
        public List<RomanizeSelection> RomanizeSelection { get; set; }
        public SelectionMode LyricSelectionMode { get; set; }
        public List<MusixMatchToken> MusixMatchToken { get; set; }
        
    }
}
