using Newtonsoft.Json;

namespace OpenLyricsClient.Shared.Structure.Json.Musixmatch.Json
{
    public class MusixMatchTrackLyricsGetMessage
    {
        [JsonProperty("header")]
        public MusixMatchTrackLyricsGetMessageHeader Header { get; set; }

        [JsonProperty("body")]
        public MusixMatchTrackLyricsGetMessageBody Body { get; set; }
    }
}