using Newtonsoft.Json;

namespace OpenLyricsClient.Backend.Structure.Json.Musixmatch.Json
{
    public class MusixMatchTrackSnippetGet
    {
        [JsonProperty("message")]
        public MusixMatchTrackSnippetGetMessage Message { get; set; }
    }
}