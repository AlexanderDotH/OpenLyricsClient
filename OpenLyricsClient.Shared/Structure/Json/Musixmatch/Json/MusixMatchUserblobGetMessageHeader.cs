using Newtonsoft.Json;

namespace OpenLyricsClient.Shared.Structure.Json.Musixmatch.Json
{
    public class MusixMatchUserblobGetMessageHeader
    {
        [JsonProperty("status_code")]
        public long StatusCode { get; set; }
    }
}