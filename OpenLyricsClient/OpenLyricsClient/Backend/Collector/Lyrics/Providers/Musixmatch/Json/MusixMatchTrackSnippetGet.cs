using Newtonsoft.Json;

namespace OpenLyricsClient.Backend.Collector.Lyrics.Providers.Musixmatch.Json
{
    public class MusixMatchTrackSnippetGet
    {
        [JsonProperty("message")]
        public MusixMatchTrackSnippetGetMessage Message { get; set; }
    }
}