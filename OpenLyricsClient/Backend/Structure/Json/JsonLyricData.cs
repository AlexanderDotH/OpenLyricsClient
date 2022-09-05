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

        [JsonProperty("Parts")]
        public LyricPart[] LyricParts { get; set; }

        [JsonProperty("FullLyrics")]
        public string FullLyrics { get; set; }
    }
}
