using Newtonsoft.Json;

namespace OpenLyricsClient.Shared.Structure.Json.Musixmatch.Json
{
    public class MusixMatchMatcherTrackGet
    {
        [JsonProperty("message")]
        public MusixMatchMatcherTrackGetMessage Message { get; set; }
    }
}