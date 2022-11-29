using Newtonsoft.Json;

namespace OpenLyricsClient.Backend.Structure.Json.Musixmatch.Json
{
    public class MusixMatchTrackLyricsGetMessageBody
    {
        [JsonProperty("lyrics")]
        public MusixMatchLyrics Lyrics { get; set; }
    }
}