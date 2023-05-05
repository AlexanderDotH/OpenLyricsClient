using Newtonsoft.Json;

namespace OpenLyricsClient.Shared.Structure.Json.Musixmatch.Json
{
    public class MusixMatchTrackSubtitlesGet
    {
        [JsonProperty("message")]
        public MusixMatchTrackSubtitlesGetMessage Message { get; set; }
    }
}