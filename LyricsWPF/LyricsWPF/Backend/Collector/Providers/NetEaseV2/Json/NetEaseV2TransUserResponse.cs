using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LyricsWPF.Backend.Collector.Providers.NetEaseV2.Json
{
    class NetEaseV2TransUserResponse
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("demand")]
        public int Demand { get; set; }

        [JsonProperty("userid")]
        public int Userid { get; set; }

        [JsonProperty("nickname")]
        public string Nickname { get; set; }

        [JsonProperty("uptime")]
        public long Uptime { get; set; }
    }
}
