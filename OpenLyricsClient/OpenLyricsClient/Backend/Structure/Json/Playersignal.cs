using Newtonsoft.Json;

namespace OpenLyricsClient.Backend.Structure.Json
{
    public class Playersignal
    {
        [JsonProperty("signal")]
        public string Signal { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }
    }
}
