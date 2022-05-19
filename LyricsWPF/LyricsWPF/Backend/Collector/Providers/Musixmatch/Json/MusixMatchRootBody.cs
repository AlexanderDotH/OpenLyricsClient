using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LyricsWPF.Backend.Collector.Providers.Musixmatch.Json;
using Newtonsoft.Json;

namespace LyricsWPF.Backend.Collector.Providers.MusixMatch.Json
{
    public class MusixMatchRootBody
    {
        [JsonProperty("macro_calls")]
        public MusixMatchMacroCalls MacroCalls { get; set; }
    }
}
