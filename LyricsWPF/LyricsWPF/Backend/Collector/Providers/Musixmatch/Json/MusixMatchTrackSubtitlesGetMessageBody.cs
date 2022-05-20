using Newtonsoft.Json;

namespace LyricsWPF.Backend.Collector.Providers.Musixmatch.Json
{
    public class MusixMatchTrackSubtitlesGetMessageBody
    {
        [JsonProperty("subtitle_list")]
        public MusixMatchSubtitleList[] SubtitleList { get; set; }
    }
}