using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LyricsWPF.Backend.Structure.Json
{
    public class Playersignal
    {
        [JsonProperty("signal")]
        public string Signal { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }
    }
}
