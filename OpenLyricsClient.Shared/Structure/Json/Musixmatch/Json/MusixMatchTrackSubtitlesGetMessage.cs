using Newtonsoft.Json;

namespace OpenLyricsClient.Shared.Structure.Json.Musixmatch.Json
{
    public class MusixMatchTrackSubtitlesGetMessage
    {
        [JsonProperty("header")]
        public MusixMatchTrackSubtitlesGetMessageHeader Header { get; set; }

        [JsonProperty("body")]
        public MusixMatchTrackSubtitlesGetMessageBody Body { get; set; }
    }
}