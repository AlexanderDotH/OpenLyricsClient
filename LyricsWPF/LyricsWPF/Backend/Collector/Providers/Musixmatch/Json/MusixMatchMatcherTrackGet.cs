using Newtonsoft.Json;

namespace LyricsWPF.Backend.Collector.Providers.Musixmatch.Json
{
    public class MusixMatchMatcherTrackGet
    {
        [JsonProperty("message")]
        public MusixMatchMatcherTrackGetMessage Message { get; set; }
    }
}