using Newtonsoft.Json;

namespace OpenLyricsClient.Backend.Collector.Lyrics.Providers.Musixmatch.Json
{
    public class MusixMatchTrackLyricsGetMessage
    {
        [JsonProperty("header")]
        public MusixMatchTrackLyricsGetMessageHeader Header { get; set; }

        [JsonProperty("body")]
        public MusixMatchTrackLyricsGetMessageBody Body { get; set; }
    }
}