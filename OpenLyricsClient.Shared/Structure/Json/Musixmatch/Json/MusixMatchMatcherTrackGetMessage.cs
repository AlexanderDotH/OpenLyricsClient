using Newtonsoft.Json;

namespace OpenLyricsClient.Shared.Structure.Json.Musixmatch.Json
{
    public class MusixMatchMatcherTrackGetMessage
    {
        [JsonProperty("header")]
        public MusixMatchMatcherTrackGetMessageHeader Header { get; set; }

        [JsonProperty("body")]
        public MusicMatchMatcherTrackGetMessageBody Body { get; set; }
    }
}