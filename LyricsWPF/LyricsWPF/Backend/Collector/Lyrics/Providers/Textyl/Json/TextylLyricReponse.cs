using Newtonsoft.Json;

namespace LyricsWPF.Backend.Collector.Lyrics.Providers.Textyl.Json
{
    public class TextylLyricReponse
    {
        [JsonProperty("seconds")]
        public int Seconds { get; set; }

        [JsonProperty("lyrics")]
        public string Lyrics { get; set; }
    }
}
