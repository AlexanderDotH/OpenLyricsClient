using Newtonsoft.Json;

namespace OpenLyricsClient.Backend.Structure.Json.Musixmatch.Json
{
    public class MusixMatchUserblobGetMessageHeader
    {
        [JsonProperty("status_code")]
        public long StatusCode { get; set; }
    }
}