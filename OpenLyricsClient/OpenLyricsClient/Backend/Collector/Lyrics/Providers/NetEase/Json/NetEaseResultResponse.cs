using Newtonsoft.Json;

namespace OpenLyricsClient.Backend.Collector.Lyrics.Providers.NetEase.Json
{
    public class NetEaseResultResponse
    {
        [JsonProperty("songs")]
        public NetEaseSongResponse[] Songs { get; set; }

        [JsonProperty("hasMore")]
        public bool HasMore { get; set; }

        [JsonProperty("songCount")]
        public int SongCount { get; set; }
    }
}