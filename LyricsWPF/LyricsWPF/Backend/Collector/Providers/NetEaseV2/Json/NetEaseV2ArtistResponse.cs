using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LyricsWPF.Backend.Collector.Providers.NetEaseV2.Json
{
    class NetEaseV2ArtistResponse
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("picUrl")]
        public object PicUrl { get; set; }

        [JsonProperty("alias")]
        public object[] Alias { get; set; }

        [JsonProperty("albumSize")]
        public int AlbumSize { get; set; }

        [JsonProperty("picId")]
        public int PicId { get; set; }

        [JsonProperty("img1v1Url")]
        public string Img1v1Url { get; set; }

        [JsonProperty("img1v1")]
        public int Img1v1 { get; set; }

        [JsonProperty("trans")]
        public object Trans { get; set; }
    }
}
