using Newtonsoft.Json;

namespace OpenLyricsClient.Backend.Structure.Json.Musixmatch.Json
{
    public class MusicMatchMatcherTrackGetMessageBody
    {
        [JsonProperty("track")]
        public MusixMatchTrack Track { get; set; }
    }
}