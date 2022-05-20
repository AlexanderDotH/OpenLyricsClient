using Newtonsoft.Json;

namespace LyricsWPF.Backend.Collector.Providers.Musixmatch.Json
{
    public class MusixMatchTrackSnippetGetMessageBody
    {
        [JsonProperty("snippet")]
        public MusixMatchSnippet Snippet { get; set; }
    }
}