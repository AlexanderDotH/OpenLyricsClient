using Newtonsoft.Json;

namespace OpenLyricsClient.Shared.Structure.Json.NetEase.Json
{
    public class NetEaseLrcResponse
    {
        [JsonProperty("version")]
        public int Version { get; set; }

        [JsonProperty("lyric")]
        public string Lyric { get; set; }
    }
}