using Newtonsoft.Json;

namespace LyricsWPF.Backend.Collector.Providers.Musixmatch.Json
{
    public class MusixMatchUserblobGet
    {
        [JsonProperty("message")]
        public MusixMatchUserblobGetMessage Message { get; set; }

        [JsonProperty("meta")]
        public MusixMatchMeta Meta { get; set; }
    }
}