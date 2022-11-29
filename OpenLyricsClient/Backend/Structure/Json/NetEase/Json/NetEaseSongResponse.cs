using Newtonsoft.Json;

namespace OpenLyricsClient.Backend.Structure.Json.NetEase.Json
{
    public class NetEaseSongResponse
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("artists")]
        public NetEaseArtistResponse[] Artists { get; set; }

        [JsonProperty("album")]
        public NetEaseAlbumResponse NetEaseAlbumResponse { get; set; }

        [JsonProperty("duration")]
        public int Duration { get; set; }

        [JsonProperty("copyrightId")]
        public int CopyrightId { get; set; }

        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("alias")]
        public object[] Alias { get; set; }

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
        public int Mark { get; set; }
    }
}