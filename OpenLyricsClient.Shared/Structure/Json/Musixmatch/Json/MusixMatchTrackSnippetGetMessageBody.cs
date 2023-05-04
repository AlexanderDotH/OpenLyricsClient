using Newtonsoft.Json;

namespace OpenLyricsClient.Shared.Structure.Json.Musixmatch.Json
{
    public class MusixMatchTrackSnippetGetMessageBody
    {
        [JsonProperty("snippet")]
        public MusixMatchSnippet Snippet { get; set; }
    }
}