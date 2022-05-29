using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LyricsWPF.Backend.Structure.Json
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
