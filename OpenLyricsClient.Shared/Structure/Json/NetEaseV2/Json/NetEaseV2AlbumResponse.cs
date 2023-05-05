using Newtonsoft.Json;

namespace OpenLyricsClient.Shared.Structure.Json.NetEaseV2.Json
{
    public class NetEaseV2AlbumResponse
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
