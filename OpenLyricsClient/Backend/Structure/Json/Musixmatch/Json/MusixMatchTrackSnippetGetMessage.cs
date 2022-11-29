using Newtonsoft.Json;

namespace OpenLyricsClient.Backend.Structure.Json.Musixmatch.Json
{
    public class MusixMatchTrackSnippetGetMessage
    {
        [JsonProperty("header")]
        public MusixMatchTrackSnippetGetMessageHeader Header { get; set; }

        [JsonProperty("body")]
        public MusixMatchTrackSnippetGetMessageBody Body { get; set; }
    }
}