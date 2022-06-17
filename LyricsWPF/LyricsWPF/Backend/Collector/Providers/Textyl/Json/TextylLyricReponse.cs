using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LyricsWPF.Backend.Collector.Providers.Textyl.Json
{
    public class TextylLyricReponse
    {
        [JsonProperty("seconds")]
        public int Seconds { get; set; }

        [JsonProperty("lyrics")]
        public string Lyrics { get; set; }
    }
}
