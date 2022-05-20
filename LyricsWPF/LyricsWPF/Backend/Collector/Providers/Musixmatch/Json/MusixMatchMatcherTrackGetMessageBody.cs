using Newtonsoft.Json;

namespace LyricsWPF.Backend.Collector.Providers.Musixmatch.Json
{
    public class MusicMatchMatcherTrackGetMessageBody
    {
        [JsonProperty("track")]
        public MusixMatchTrack Track { get; set; }
    }
}