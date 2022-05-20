using Newtonsoft.Json;

namespace LyricsWPF.Backend.Collector.Providers.Musixmatch.Json
{
    public class MusixMatchUserblobGetMessage
    {
        [JsonProperty("header")]
        public MusixMatchUserblobGetMessageHeader Header { get; set; }
    }
}