using Newtonsoft.Json;
using OpenLyricsClient.Backend.Structure.Lyrics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenLyricsClient.Backend.Structure.Json
{
    public class JsonLyricData
    {
        [JsonProperty("Type")]
        public LyricType LyricType { get; set; }

        [JsonProperty("ReturnCode")]
        public LyricReturnCode ReturnCode { get; set; }
        
        [JsonProperty("Provider")]
        public string LyricProvider { get; set; }

        [JsonProperty("Parts")]
        public LyricPart[] LyricParts { get; set; }
    }
}
