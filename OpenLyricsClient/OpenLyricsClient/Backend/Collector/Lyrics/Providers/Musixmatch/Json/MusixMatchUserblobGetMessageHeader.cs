using Newtonsoft.Json;

namespace OpenLyricsClient.Backend.Collector.Lyrics.Providers.Musixmatch.Json
{
    public class MusixMatchUserblobGetMessageHeader
    {
        [JsonProperty("status_code")]
        public long StatusCode { get; set; }
    }
}