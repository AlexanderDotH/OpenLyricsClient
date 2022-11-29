using Newtonsoft.Json;

namespace OpenLyricsClient.Backend.Structure.Json.Musixmatch.Json
{
    public class MusixMatchUserblobGetMessage
    {
        [JsonProperty("header")]
        public MusixMatchUserblobGetMessageHeader Header { get; set; }
    }
}