using Newtonsoft.Json;

namespace OpenLyricsClient.Shared.Structure.Json.Musixmatch.Json
{
    public class MusixMatchUserblobGet
    {
        [JsonProperty("message")]
        public MusixMatchUserblobGetMessage Message { get; set; }

        [JsonProperty("meta")]
        public MusixMatchMeta Meta { get; set; }
    }
}