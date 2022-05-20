using Newtonsoft.Json;

namespace LyricsWPF.Backend.Collector.Providers.Musixmatch.Json
{
    public class MusixMatchTrackSnippetGetMessage
    {
        [JsonProperty("header")]
        public MusixMatchTrackSnippetGetMessageHeader Header { get; set; }

        [JsonProperty("body")]
        public MusixMatchTrackSnippetGetMessageBody Body { get; set; }
    }
}