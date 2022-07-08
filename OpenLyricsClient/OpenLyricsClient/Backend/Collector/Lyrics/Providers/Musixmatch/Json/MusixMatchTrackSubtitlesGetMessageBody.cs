using Newtonsoft.Json;

namespace OpenLyricsClient.Backend.Collector.Lyrics.Providers.Musixmatch.Json
{
    public class MusixMatchTrackSubtitlesGetMessageBody
    {
        [JsonProperty("subtitle_list")]
        public MusixMatchSubtitleList[] SubtitleList { get; set; }
    }
}