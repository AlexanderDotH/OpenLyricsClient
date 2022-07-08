using Newtonsoft.Json;

namespace OpenLyricsClient.Backend.Collector.Lyrics.Providers.Musixmatch.Json
{
    public class MusicMatchMatcherTrackGetMessageBody
    {
        [JsonProperty("track")]
        public MusixMatchTrack Track { get; set; }
    }
}