using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LyricsWPF.Backend.Collector.Providers.NetEase.Json
{
    public class NetEaseLyricResponse
    {
        [JsonProperty("sgc")]
        public bool Sgc { get; set; }

        [JsonProperty("sfy")]
        public bool Sfy { get; set; }

        [JsonProperty("qfy")]
        public bool Qfy { get; set; }

        [JsonProperty("transUser")]
        public NetEaseTransUserResponse NetEaseTransUserResponse { get; set; }

        [JsonProperty("lyricUser")]
        public NetEaseLyricUserResponse NetEaseLyricUserResponse { get; set; }

        [JsonProperty("lrc")]
        public NetEaseLrcResponse NetEaseLrcResponse { get; set; }

        [JsonProperty("klyric")]
        public NetEaseKlyricResponse NetEaseKlyricResponse { get; set; }

        [JsonProperty("tlyric")]
        public NetEaseTlyricResponse NetEaseTlyricResponse { get; set; }

        [JsonProperty("code")]
        public int Code { get; set; }
    }
}
