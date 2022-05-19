using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LyricsWPF.Backend.Collector.Providers.Musixmatch.Json
{
    public class MusixMatchRootHeader
    {
        [JsonProperty("status_code")]
        public int StatusCode { get; set; }

        [JsonProperty("execute_time")]
        public double ExecuteTime { get; set; }

        [JsonProperty("pid")]
        public int Pid { get; set; }

        [JsonProperty("surrogate_key_list")]
        public object[] SurrogateKeyList { get; set; }
    }
}
