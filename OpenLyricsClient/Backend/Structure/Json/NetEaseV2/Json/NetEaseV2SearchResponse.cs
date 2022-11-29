using Newtonsoft.Json;

namespace OpenLyricsClient.Backend.Structure.Json.NetEaseV2.Json
{
    class NetEaseV2SearchResponse
    {
        [JsonProperty("result")]
        public NetEaseV2ResultResponse Result { get; set; }

        [JsonProperty("code")]
        public int Code { get; set; }
    }
}
