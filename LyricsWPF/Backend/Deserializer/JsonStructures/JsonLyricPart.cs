using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LyricsWPF.Backend.Deserializer.JsonStructures
{
    public class JsonLyricPart
    {
        [JsonProperty("seconds")]
        public int seconds { get; set; }

        [JsonProperty("lyrics")]
        public string lyrics { get; set; }
    }
}
