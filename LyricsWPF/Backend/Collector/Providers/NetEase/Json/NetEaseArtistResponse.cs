using System.Collections.Generic;
using Newtonsoft.Json;

namespace LyricsWPF.Backend.Collector.Providers.NetEase.Json
{
    public class NetEaseArtistResponse
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("picUrl")]
        public object PicUrl { get; set; }

        [JsonProperty("alias")]
        public List<object> Alias { get; set; }

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