using Newtonsoft.Json;

namespace OpenLyricsClient.Shared.Structure.Json.Musixmatch.Json
{
    public class MusixMatchTrackLyricsGet
    {
        [JsonProperty("message")]
        public MusixMatchTrackLyricsGetMessage Message { get; set; }
    }
}