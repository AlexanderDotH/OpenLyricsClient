using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LyricsWPF.Backend.Collector.Providers.NetEaseV2.Json
{
    class NetEaseV2AlbumResponse
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("artist")]
        public NetEaseV2ArtistResponse Artist { get; set; }

        [JsonProperty("publishTime")]
        public long PublishTime { get; set; }

        [JsonProperty("size")]
        public int Size { get; set; }

        [JsonProperty("copyrightId")]
        public int CopyrightId { get; set; }

        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("picId")]
        public long PicId { get; set; }

        [JsonProperty("mark")]
        public int Mark { get; set; }
    }
}
