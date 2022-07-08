using Newtonsoft.Json;

namespace OpenLyricsClient.Backend.Collector.Lyrics.Providers.NetEaseV2.Json
{
    class NetEaseV2KlyricResponse
    {
        [JsonProperty("version")]
        public int Version { get; set; }

        [JsonProperty("lyric")]
        public string Lyric { get; set; }
    }
}
