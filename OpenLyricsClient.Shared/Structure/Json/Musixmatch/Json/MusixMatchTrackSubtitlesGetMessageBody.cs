using Newtonsoft.Json;

namespace OpenLyricsClient.Shared.Structure.Json.Musixmatch.Json
{
    public class MusixMatchTrackSubtitlesGetMessageBody
    {
        [JsonProperty("subtitle_list")]
        public MusixMatchSubtitleList[] SubtitleList { get; set; }
    }
}