using Newtonsoft.Json;

namespace OpenLyricsClient.Shared.Structure.Json.Musixmatch.Json
{
    public class MusixMatchUserblobGetMessage
    {
        [JsonProperty("header")]
        public MusixMatchUserblobGetMessageHeader Header { get; set; }
    }
}