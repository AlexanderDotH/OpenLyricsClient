using Newtonsoft.Json;

namespace OpenLyricsClient.Backend.Structure.Json
{
    public class JsonTidalResponse
    {
        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("error")]
        public object Error { get; set; }

        [JsonProperty("sub_status")]
        public int SubStatus { get; set; }

        [JsonProperty("error_description")]
        public string ErrorDescription { get; set; }
    }
}
