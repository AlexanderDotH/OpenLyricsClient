using Newtonsoft.Json;

namespace LyricsWPF.Backend.Collector.Providers.Musixmatch.Json
{
    public class MusixMatchMatcherTrackGetMessage
    {
        [JsonProperty("header")]
        public MusixMatchMatcherTrackGetMessageHeader Header { get; set; }

        [JsonProperty("body")]
        public MusicMatchMatcherTrackGetMessageBody Body { get; set; }
    }
}