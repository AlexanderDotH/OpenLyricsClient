using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LyricsWPF.Backend.Structure.Json
{
    public class JsonTidalAuthDevice
    {
        [JsonProperty("deviceCode")]
        public string DeviceCode { get; set; }

        [JsonProperty("userCode")]
        public string UserCode { get; set; }

        [JsonProperty("verificationUri")]
        public string VerificationUri { get; set; }

        [JsonProperty("verificationUriComplete")]
        public string VerificationUriComplete { get; set; }

        [JsonProperty("expiresIn")]
        public int ExpiresIn { get; set; }

        [JsonProperty("interval")]
        public int Interval { get; set; }
    }
}
