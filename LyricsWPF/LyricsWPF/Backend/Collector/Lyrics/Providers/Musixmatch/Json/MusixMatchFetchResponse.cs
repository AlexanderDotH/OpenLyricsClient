using Newtonsoft.Json;

namespace LyricsWPF.Backend.Collector.Lyrics.Providers.Musixmatch.Json
{
    public class MusixMatchFetchResponse
    {
        [JsonProperty("message")]
        public MusixMatchFetchResponseMessage Message { get; set; }
    }
}
