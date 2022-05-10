using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LyricsWPF.Backend.Collector.Providers.NetEaseV2.Json
{
    class NetEaseV2SongResponse
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("artists")]
        public List<NetEaseV2ArtistResponse> Artists { get; set; }

        [JsonProperty("album")]
        public NetEaseV2AlbumResponse Album { get; set; }

        [JsonProperty("duration")]
        public int Duration { get; set; }

        [JsonProperty("copyrightId")]
        public int CopyrightId { get; set; }

        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("alias")]
        public List<object> Alias { get; set; }

        [JsonProperty("rtype")]
        public int Rtype { get; set; }

        [JsonProperty("ftype")]
        public int Ftype { get; set; }

        [JsonProperty("mvid")]
        public int Mvid { get; set; }

        [JsonProperty("fee")]
        public int Fee { get; set; }

        [JsonProperty("rUrl")]
        public object RUrl { get; set; }

        [JsonProperty("mark")]
        public long Mark { get; set; }
    }
}
