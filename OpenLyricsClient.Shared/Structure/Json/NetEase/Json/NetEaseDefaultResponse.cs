using Newtonsoft.Json;

namespace OpenLyricsClient.Shared.Structure.Json.NetEase.Json
{
    public class NetEaseDefaultResponse
    {
        [JsonProperty("msg")]
        public string msg { get; set; }

        [JsonProperty("code")]
        public int code { get; set; }
    }
}
