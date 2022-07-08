using System.Collections.Generic;
using OpenLyricsClient.Backend.Collector.Lyrics;
using OpenLyricsClient.Backend.Romanisation;
using OpenLyricsClient.Backend.Structure;

namespace OpenLyricsClient.Backend.Settings
{
    public class Settings
    {
        public SpotifyAccess SpotifyAccess { get; set; }
        public TidalAccess TidalAccess { get; set; }
        public List<RomanizeSelection> RomanizeSelection { get; set; }
        public SelectionMode LyricSelectionMode { get; set; }
        public List<MusixMatchToken> MusixMatchTokens { get; set; }
    }
}
