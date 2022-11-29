using Newtonsoft.Json;

namespace OpenLyricsClient.Backend.Structure.Json.NetEaseV2.Json
{
    class NetEaseV2ResultResponse
    {
        [JsonProperty("songs")]
        public NetEaseV2SongResponse[] Songs { get; set; }

        [JsonProperty("hasMore")]
        public bool HasMore { get; set; }

        [JsonProperty("songCount")]
        public int SongCount { get; set; }
    }
}
