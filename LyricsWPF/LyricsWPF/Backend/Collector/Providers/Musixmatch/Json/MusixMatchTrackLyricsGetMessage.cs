using Newtonsoft.Json;

namespace LyricsWPF.Backend.Collector.Providers.Musixmatch.Json
{
    public class MusixMatchTrackLyricsGetMessage
    {
        [JsonProperty("header")]
        public MusixMatchTrackLyricsGetMessageHeader Header { get; set; }

        [JsonProperty("body")]
        public MusixMatchTrackLyricsGetMessageBody Body { get; set; }
    }
}