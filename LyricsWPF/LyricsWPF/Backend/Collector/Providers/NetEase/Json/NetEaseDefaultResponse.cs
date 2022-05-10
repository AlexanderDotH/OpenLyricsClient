using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LyricsWPF.Backend.Collector.Providers.NetEase.Json
{
    public class NetEaseDefaultResponse
    {
        [JsonProperty("msg")]
        public string msg { get; set; }

        [JsonProperty("code")]
        public int code { get; set; }
    }
}
