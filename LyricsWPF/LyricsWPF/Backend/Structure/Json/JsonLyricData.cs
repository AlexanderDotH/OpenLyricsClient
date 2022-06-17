using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LyricsWPF.Backend.Structure
{
    public class JsonLyricData
    {
        [JsonProperty("SongName")]
        public string SongName { get; set; }

        [JsonProperty("Album")]
        public string Album { get; set; }

        [JsonProperty("Artists")]
        public string[] Artists { get; set; }

        [JsonProperty("Duration")]
        public long Duration { get; set; }

        [JsonProperty("Type")]
        public LyricType LyricType { get; set; }

        [JsonProperty("Provider")]
        public string LyricProvider { get; set; }

        [JsonProperty("ReturnCode")]
        public LyricReturnCode LyricReturnCode { get; set; }

        [JsonProperty("Parts")]
        public LyricPart[] LyricParts { get; set; }

        [JsonProperty("FullLyrics")]
        public string FullLyrics { get; set; }
    }
}
