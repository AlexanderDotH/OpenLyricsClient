using Newtonsoft.Json;

namespace LyricsWPF.Backend.Collector.Lyrics.Providers.NetEase.Json
{
    public class NetEaseLrcResponse
    {
        [JsonProperty("version")]
        public int Version { get; set; }

        [JsonProperty("lyric")]
        public string Lyric { get; set; }
    }
}