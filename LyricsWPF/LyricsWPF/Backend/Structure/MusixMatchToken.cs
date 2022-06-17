using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LyricsWPF.Backend.Structure
{
    public class MusixMatchToken
    {
        [JsonProperty("Token")]
        public string Token { get; set; }

        [JsonProperty("ExpirationDate")]
        public long ExpirationDate { get; set; }
    }
}
