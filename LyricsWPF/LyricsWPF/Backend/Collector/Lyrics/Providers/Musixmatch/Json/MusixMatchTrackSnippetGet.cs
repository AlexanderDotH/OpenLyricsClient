using Newtonsoft.Json;

namespace LyricsWPF.Backend.Collector.Lyrics.Providers.Musixmatch.Json
{
    public class MusixMatchTrackSnippetGet
    {
        [JsonProperty("message")]
        public MusixMatchTrackSnippetGetMessage Message { get; set; }
    }
}