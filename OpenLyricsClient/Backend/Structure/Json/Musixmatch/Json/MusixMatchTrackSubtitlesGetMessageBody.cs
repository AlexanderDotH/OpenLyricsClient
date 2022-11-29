using Newtonsoft.Json;

namespace OpenLyricsClient.Backend.Structure.Json.Musixmatch.Json
{
    public class MusixMatchTrackSubtitlesGetMessageBody
    {
        [JsonProperty("subtitle_list")]
        public MusixMatchSubtitleList[] SubtitleList { get; set; }
    }
}