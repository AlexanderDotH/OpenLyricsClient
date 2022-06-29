using Newtonsoft.Json;

namespace LyricsWPF.Backend.Collector.Lyrics.Providers.Musixmatch.Json
{
    public class MusixMatchMatcherTrackGet
    {
        [JsonProperty("message")]
        public MusixMatchMatcherTrackGetMessage Message { get; set; }
    }
}