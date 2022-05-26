using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LyricsWPF.Backend.Structure.Json
{
    public class JsonTidalAccess
    {
        [JsonProperty("apiToken")]
        public string ApiToken { get; set; }

        [JsonProperty("clientId")]
        public object ClientId { get; set; }

        [JsonProperty("clientUniqueKey")]
        public string ClientUniqueKey { get; set; }

        [JsonProperty("countryCode")]
        public string CountryCode { get; set; }

        [JsonProperty("facebookAccessToken")]
        public object FacebookAccessToken { get; set; }

        [JsonProperty("isLoading")]
        public bool IsLoading { get; set; }

        [JsonProperty("isPolling")]
        public bool IsPolling { get; set; }

        [JsonProperty("oAuthAccessToken")]
        public string OAuthAccessToken { get; set; }

        [JsonProperty("oAuthExpirationDate")]
        public long OAuthExpirationDate { get; set; }

        [JsonProperty("oAuthRefreshToken")]
        public string OAuthRefreshToken { get; set; }

        [JsonProperty("userId")]
        public int UserId { get; set; }

        [JsonProperty("username")]
        public object Username { get; set; }

        [JsonProperty("utmParameters")]
        public JsonTidalUtmParameters UtmParameters { get; set; }
    }
}
