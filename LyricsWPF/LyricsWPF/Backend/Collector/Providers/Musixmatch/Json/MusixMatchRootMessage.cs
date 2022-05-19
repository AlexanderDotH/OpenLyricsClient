using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LyricsWPF.Backend.Collector.Providers.MusixMatch.Json;
using Newtonsoft.Json;

namespace LyricsWPF.Backend.Collector.Providers.Musixmatch.Json
{
    public class MusixMatchRootMessage
    {
        [JsonProperty("header")]
        public MusixMatchRootHeader RootHeader { get; set; }

        [JsonProperty("body")]
        public MusixMatchRootBody RootBody { get; set; }
    }
}
