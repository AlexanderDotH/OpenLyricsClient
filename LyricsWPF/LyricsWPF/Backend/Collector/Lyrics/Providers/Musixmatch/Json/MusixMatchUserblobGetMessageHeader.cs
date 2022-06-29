using Newtonsoft.Json;

namespace LyricsWPF.Backend.Collector.Lyrics.Providers.Musixmatch.Json
{
    public class MusixMatchUserblobGetMessageHeader
    {
        [JsonProperty("status_code")]
        public long StatusCode { get; set; }
    }
}