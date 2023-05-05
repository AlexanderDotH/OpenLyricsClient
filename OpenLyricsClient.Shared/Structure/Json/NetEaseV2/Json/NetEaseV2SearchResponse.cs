using Newtonsoft.Json;

namespace OpenLyricsClient.Shared.Structure.Json.NetEaseV2.Json
{
    public class NetEaseV2SearchResponse
    {
        [JsonProperty("result")]
        public NetEaseV2ResultResponse Result { get; set; }

        [JsonProperty("code")]
        public int Code { get; set; }
    }
}
