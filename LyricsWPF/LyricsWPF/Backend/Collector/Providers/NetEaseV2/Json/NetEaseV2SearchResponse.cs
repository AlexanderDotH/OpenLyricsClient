using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LyricsWPF.Backend.Collector.Providers.NetEaseV2.Json
{
    class NetEaseV2SearchResponse
    {
        [JsonProperty("result")]
        public NetEaseV2ResultResponse Result { get; set; }

        [JsonProperty("code")]
        public int Code { get; set; }
    }
}
