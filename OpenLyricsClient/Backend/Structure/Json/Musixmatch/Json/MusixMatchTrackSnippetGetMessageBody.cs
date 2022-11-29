using Newtonsoft.Json;

namespace OpenLyricsClient.Backend.Structure.Json.Musixmatch.Json
{
    public class MusixMatchTrackSnippetGetMessageBody
    {
        [JsonProperty("snippet")]
        public MusixMatchSnippet Snippet { get; set; }
    }
}