using Newtonsoft.Json;

namespace OpenLyricsClient.Shared.Structure.Json.NetEase.Json
{
    public class NetEaseLyricUserResponse
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