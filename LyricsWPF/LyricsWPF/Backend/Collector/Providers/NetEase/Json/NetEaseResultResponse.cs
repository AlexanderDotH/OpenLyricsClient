using System.Collections.Generic;
using Newtonsoft.Json;

namespace LyricsWPF.Backend.Collector.Providers.NetEase.Json
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