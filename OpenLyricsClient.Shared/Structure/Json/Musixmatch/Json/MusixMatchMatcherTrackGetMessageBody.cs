using Newtonsoft.Json;

namespace OpenLyricsClient.Shared.Structure.Json.Musixmatch.Json
{
    public class MusicMatchMatcherTrackGetMessageBody
    {
        [JsonProperty("track")]
        public MusixMatchTrack Track { get; set; }
    }
}