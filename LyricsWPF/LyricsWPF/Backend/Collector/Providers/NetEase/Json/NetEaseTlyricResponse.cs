using Newtonsoft.Json;

namespace LyricsWPF.Backend.Collector.Providers.NetEase.Json
{
    public class NetEaseTlyricResponse
    {
        [JsonProperty("version")]
        public int Version { get; set; }

        [JsonProperty("lyric")]
        public string Lyric { get; set; }
    }
}