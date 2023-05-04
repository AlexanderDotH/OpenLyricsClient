using Newtonsoft.Json;

namespace OpenLyricsClient.Shared.Structure.Json.NetEaseV2.Json
{
    public class NetEaseV2ResultResponse
    {
        [JsonProperty("songs")]
        public NetEaseV2SongResponse[] Songs { get; set; }

        [JsonProperty("hasMore")]
        public bool HasMore { get; set; }

        [JsonProperty("songCount")]
        public int SongCount { get; set; }
    }
}
