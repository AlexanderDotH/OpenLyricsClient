using Newtonsoft.Json;

namespace LyricsWPF.Backend.Collector.Lyrics.Providers.Musixmatch.Json
{
    public class MusixMatchTrackSubtitlesGetMessage
    {
        [JsonProperty("header")]
        public MusixMatchTrackSubtitlesGetMessageHeader Header { get; set; }

        [JsonProperty("body")]
        public MusixMatchTrackSubtitlesGetMessageBody Body { get; set; }
    }
}