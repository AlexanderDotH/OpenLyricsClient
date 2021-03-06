using Newtonsoft.Json;

namespace OpenLyricsClient.Backend.Collector.Lyrics.Providers.NetEaseV2.Json
{
    class NetEaseV2TlyricResponse
    {
        [JsonProperty("version")]
        public int Version { get; set; }

        [JsonProperty("lyric")]
        public string Lyric { get; set; }
    }
}
