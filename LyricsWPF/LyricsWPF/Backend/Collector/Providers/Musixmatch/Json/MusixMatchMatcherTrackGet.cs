using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LyricsWPF.Backend.Collector.Providers.Musixmatch.Json
{
    public class MusixMatchMatcherTrackGet
    {
        [JsonProperty("message")]
        public MusixMatchMatcherTrackGetMessage Message { get; set; }
    }
}
