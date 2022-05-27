using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LyricsWPF.Backend.Structure
{
    public class TidalAccess
    {
        [JsonProperty("isTidalConnected")]
        public bool IsTidalConnected { get; set; }

        [JsonProperty("userId")]
        public int UserID { get; set; }

        [JsonProperty("apiToken")]
        public string ApiToken { get; set; }

        [JsonProperty("clientUniqueKey")]
        public string UniqueKey { get; set; }

        [JsonProperty("oAuthAccessToken")]
        public string AccessToken { get; set; }

        [JsonProperty("oAuthRefreshToken")]
        public string RefreshToken { get; set; }

        [JsonProperty("oAuthExpirationDate")]
        public long ExpirationDate { get; set; }
    }
}
