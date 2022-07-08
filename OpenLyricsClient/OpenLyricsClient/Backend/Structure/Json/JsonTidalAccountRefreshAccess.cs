using Newtonsoft.Json;

namespace OpenLyricsClient.Backend.Structure.Json
{
    public class JsonTidalAccountRefreshAccess
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty("user")]
        public JsonTidalUser User { get; set; }
    }
}
