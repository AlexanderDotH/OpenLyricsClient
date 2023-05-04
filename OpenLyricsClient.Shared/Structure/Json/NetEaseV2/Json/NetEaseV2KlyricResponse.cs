using Newtonsoft.Json;

namespace OpenLyricsClient.Shared.Structure.Json.NetEaseV2.Json
{
    public class NetEaseV2KlyricResponse
    {
        [JsonProperty("version")]
        public int Version { get; set; }

        [JsonProperty("lyric")]
        public string Lyric { get; set; }
    }
}
