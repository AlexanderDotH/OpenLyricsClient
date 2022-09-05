using Newtonsoft.Json;

namespace OpenLyricsClient.Backend.Collector.Lyrics.Providers.Musixmatch.Json
{
    public class MusixMatchTrackLyricsGet
    {
        [JsonProperty("message")]
        public MusixMatchTrackLyricsGetMessage Message { get; set; }
    }
}