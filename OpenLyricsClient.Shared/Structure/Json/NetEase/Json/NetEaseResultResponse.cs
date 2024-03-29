﻿using Newtonsoft.Json;

namespace OpenLyricsClient.Shared.Structure.Json.NetEase.Json
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