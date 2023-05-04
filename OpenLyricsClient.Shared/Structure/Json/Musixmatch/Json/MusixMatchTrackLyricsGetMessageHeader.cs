using Newtonsoft.Json;

namespace OpenLyricsClient.Shared.Structure.Json.Musixmatch.Json
{
    public class MusixMatchTrackLyricsGetMessageHeader
    {
        [JsonProperty("status_code")]
        public long StatusCode { get; set; }

        [JsonProperty("execute_time")]
        public double ExecuteTime { get; set; }
    }
}