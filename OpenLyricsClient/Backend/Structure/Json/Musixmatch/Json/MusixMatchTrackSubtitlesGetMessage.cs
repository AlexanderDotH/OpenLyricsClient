using Newtonsoft.Json;

namespace OpenLyricsClient.Backend.Structure.Json.Musixmatch.Json
{
    public class MusixMatchTrackSubtitlesGetMessage
    {
        [JsonProperty("header")]
        public MusixMatchTrackSubtitlesGetMessageHeader Header { get; set; }

        [JsonProperty("body")]
        public MusixMatchTrackSubtitlesGetMessageBody Body { get; set; }
    }
}