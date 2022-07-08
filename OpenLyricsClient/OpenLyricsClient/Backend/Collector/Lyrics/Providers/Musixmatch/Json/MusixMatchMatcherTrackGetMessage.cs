using Newtonsoft.Json;

namespace OpenLyricsClient.Backend.Collector.Lyrics.Providers.Musixmatch.Json
{
    public class MusixMatchMatcherTrackGetMessage
    {
        [JsonProperty("header")]
        public MusixMatchMatcherTrackGetMessageHeader Header { get; set; }

        [JsonProperty("body")]
        public MusicMatchMatcherTrackGetMessageBody Body { get; set; }
    }
}