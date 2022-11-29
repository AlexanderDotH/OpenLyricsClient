using Newtonsoft.Json;

namespace OpenLyricsClient.Backend.Structure.Json.NetEaseV2.Json
{
    class NetEaseV2LyricResponse
    {
        [JsonProperty("sgc")]
        public bool Sgc { get; set; }

        [JsonProperty("sfy")]
        public bool Sfy { get; set; }

        [JsonProperty("qfy")]
        public bool Qfy { get; set; }

        [JsonProperty("transUser")]
        public NetEaseV2TransUserResponse TransUser { get; set; }

        [JsonProperty("lyricUser")]
        public NetEaseV2LyricUserResponse LyricUser { get; set; }

        [JsonProperty("lrc")]
        public NetEaseV2LrcResponse Lrc { get; set; }

        [JsonProperty("klyric")]
        public NetEaseV2KlyricResponse Klyric { get; set; }

        [JsonProperty("tlyric")]
        public NetEaseV2TlyricResponse Tlyric { get; set; }

        [JsonProperty("code")]
        public int Code { get; set; }
    }
}
