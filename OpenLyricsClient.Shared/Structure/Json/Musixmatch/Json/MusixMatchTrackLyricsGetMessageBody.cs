using Newtonsoft.Json;

namespace OpenLyricsClient.Shared.Structure.Json.Musixmatch.Json
{
    public class MusixMatchTrackLyricsGetMessageBody
    {
        [JsonProperty("lyrics")]
        public MusixMatchLyrics Lyrics { get; set; }
    }
}