using Newtonsoft.Json;

namespace OpenLyricsClient.Backend.Structure.Json
{
    public class JsonTidalUtmParameters
    {
        [JsonProperty("banner")]
        public string Banner { get; set; }

        [JsonProperty("campaign")]
        public string Campaign { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("medium")]
        public string Medium { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }
    }
}