using Newtonsoft.Json;

namespace OpenLyricsClient.Backend.Structure.Json.Musixmatch.Json
{
    public class MusixMatchMatcherTrackGet
    {
        [JsonProperty("message")]
        public MusixMatchMatcherTrackGetMessage Message { get; set; }
    }
}