using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LyricsWPF.Backend.Collector.Providers.Musixmatch.Json
{
    public class MusixMatchTrackSnippetGetMessageHeader
    {
        [JsonProperty("status_code")]
        public int StatusCode { get; set; }

        [JsonProperty("execute_time")]
        public double ExecuteTime { get; set; }
    }
}
