using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LyricsWPF.Backend.Collector.Providers.Musixmatch.Json
{
    public class MusixMatchTrackLyricsGetBody
    {
        [JsonProperty("lyrics")]
        public MusixMatchTrackLyricsGetBodyLyrics Lyrics { get; set; }
    }
}
